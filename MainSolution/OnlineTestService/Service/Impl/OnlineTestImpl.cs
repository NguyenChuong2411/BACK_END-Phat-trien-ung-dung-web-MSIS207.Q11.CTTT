using Microsoft.EntityFrameworkCore;
using ModelClass.connection;
using ModelClass.OnlineTest;
using OnlineTestService.Dtos;
using System.Security.Claims;
using System.Text.Json;

namespace OnlineTestService.Service.Impl
{
    file class SimpleAnswerDto
    {
        public string Answer { get; set; }
    }

    public class OnlineTestImpl : IOnlineTest
    {
        private readonly OnlineTestDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OnlineTestImpl(OnlineTestDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<TestListItemDto>> GetAllTestsAsync()
        {
            return await _context.Tests
                .AsNoTracking()
                .Include(t => t.TestType)
                .Include(t => t.TestAttempts)
                .Include(t => t.SkillType)
                .Select(t => new TestListItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Type = t.TestType.Name,
                    SkillTypeId = t.SkillTypeId,
                    SkillName = t.SkillType.Name,
                    Description = t.Description,
                    Duration = $"{t.DurationMinutes} phút",
                    Questions = $"{t.TotalQuestions} câu hỏi",
                    Attempts = t.TestAttempts.Count()
                })
                .ToListAsync();
        }

        public async Task<FullTestDto?> GetTestDetailsByIdAsync(int testId)
        {
            // Eager loading tất cả dữ liệu liên quan để tối ưu hiệu năng
            var test = await _context.Tests
                .AsNoTracking()
                .Include(t => t.Passages.OrderBy(p => p.DisplayOrder))
                    .ThenInclude(p => p.Questions.OrderBy(q => q.QuestionNumber))
                        .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
                .Include(t => t.WritingTasks.OrderBy(w => w.DisplayOrder))
                .Include(t => t.SpeakingQuestions.OrderBy(s => s.DisplayOrder))
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null) return null;

            // Map từ entity sang DTO
            return new FullTestDto
            {
                Id = test.Id,
                Title = test.Title,
                SkillTypeId = test.SkillTypeId,
                Passages = test.Passages.Select(p => new PassageDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    Questions = p.Questions.Select(MapQuestionToDto).ToList()
                }).ToList(),
                WritingTasks = test.WritingTasks.Select(w => new WritingTaskDto
                {
                    Id = w.Id,
                    TaskType = w.TaskType,
                    Prompt = w.Prompt,
                    DurationMinutes = w.DurationMinutes ?? 0,
                    MinWords = w.MinWords ?? 0
                }).ToList(),
                SpeakingQuestions = test.SpeakingQuestions.Select(s => new SpeakingQuestionDto
                {
                    Id = s.Id,
                    PartName = s.PartName,
                    QuestionText = s.QuestionText,
                    PreparationTime = s.PreparationTimeSeconds ?? 0,
                    ResponseTime = s.ResponseTimeSeconds ?? 0
                }).ToList()
            };
        }

        public async Task<ListeningTestDto?> GetListeningTestDetailsByIdAsync(int testId)
        {
            var test = await _context.Tests
                .AsNoTracking()
                .Include(t => t.AudioFile)
                .Include(t => t.ListeningParts.OrderBy(p => p.PartNumber))
                    .ThenInclude(p => p.QuestionGroups.OrderBy(g => g.DisplayOrder))
                        .ThenInclude(g => g.Questions.OrderBy(q => q.QuestionNumber))
                            .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
                .Include(t => t.Passages.OrderBy(p => p.DisplayOrder))
                    .ThenInclude(p => p.Questions.OrderBy(q => q.QuestionNumber))
                        .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null || test.AudioFile == null) return null;

            // Xây dựng URL đầy đủ cho file audio
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fullAudioUrl = $"{baseUrl}{test.AudioFile.StoragePath}";

            return new ListeningTestDto
            {
                Id = test.Id,
                Title = test.Title,
                SkillTypeId = test.SkillTypeId,
                AudioUrl = fullAudioUrl,
                Parts = test.ListeningParts.Select(p => new ListeningPartDto
                {
                    Id = p.Id,
                    PartNumber = p.PartNumber,
                    Title = p.Title,
                    QuestionGroups = p.QuestionGroups.Select(g => new QuestionGroupDto
                    {
                        Id = g.Id,
                        InstructionText = g.InstructionText,
                        Questions = g.Questions.Select(MapQuestionToDto).ToList()
                    }).ToList()
                }).ToList(),
                Passages = test.Passages.Select(p => new PassageDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    Questions = p.Questions.Select(MapQuestionToDto).ToList()
                }).ToList()
            };
        }

        public async Task<int> SubmitTestAsync(TestSubmissionDto submission)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Tải dữ liệu Test và Questions (Giữ nguyên Include)
                var test = await _context.Tests
                    .Include(t => t.Passages).ThenInclude(p => p.Questions)
                    .Include(t => t.ListeningParts).ThenInclude(lp => lp.QuestionGroups).ThenInclude(qg => qg.Questions)
                    .AsSplitQuery() // Giữ lại AsSplitQuery nếu cần cho hiệu năng
                    .FirstOrDefaultAsync(t => t.Id == submission.TestId);

                if (test == null) throw new KeyNotFoundException("Không tìm thấy bài test.");

                var allQuestions = test.Passages.SelectMany(p => p.Questions)
                                    .Concat(test.ListeningParts.SelectMany(lp => lp.QuestionGroups.SelectMany(qg => qg.Questions)))
                                    .ToDictionary(q => q.Id); // Dùng Dictionary để truy cập nhanh

                int totalPoints = 0;
                foreach (var q in allQuestions.Values.Where(q => q.QuestionType != "table-child"))
                {
                    totalPoints += GetQuestionPointCount(q.CorrectAnswers); // Giữ nguyên hàm tính điểm
                }

                var attempt = new TestAttempt
                {
                    TestId = submission.TestId,
                    UserId = userId,
                    SubmittedAt = DateTime.UtcNow,
                    TotalQuestions = totalPoints, // Tổng điểm tối đa
                    Score = 0 // Sẽ cập nhật sau
                };
                _context.TestAttempts.Add(attempt);
                await _context.SaveChangesAsync(); // Lưu attempt để lấy ID

                int score = 0;
                var userAnswersToSave = new List<UserAnswer>();

                // Lặp qua các câu trả lời mà người dùng đã gửi
                foreach (var userAnswerPair in submission.Answers)
                {
                    // Key có thể là "questionId" hoặc "q<questionId>_<answerId>"
                    string key = userAnswerPair.Key;
                    object answerValue = userAnswerPair.Value; // Đây là câu trả lời của user

                    // Xác định Question ID
                    int questionId;
                    if (key.StartsWith("q") && key.Contains("_"))
                    {
                        // Là câu trả lời cho ô trong bảng
                        string[] parts = key.Split('_');
                        if (!int.TryParse(parts[0].Substring(1), out questionId)) continue;
                    }
                    else
                    {
                        // Là câu trả lời cho câu hỏi thường
                        if (!int.TryParse(key, out questionId)) continue;
                    }

                    // Lấy câu hỏi từ Dictionary
                    if (!allQuestions.TryGetValue(questionId, out var question)) continue;

                    // --- Xử lý lưu UserAnswer ---
                    // Chỉ lưu một UserAnswer cho mỗi câu hỏi gốc (kể cả bảng)
                    if (userAnswersToSave.All(ua => ua.QuestionId != questionId))
                    {
                        // Tạo JsonDocument cho câu trả lời của người dùng
                        JsonDocument userAnswerJsonDoc = GetUserAnswerForSaving(question, submission.Answers);

                        // Chấm điểm (chỉ chấm câu hỏi gốc, không chấm table-child)
                        bool isCorrect = false;
                        if (question.QuestionType != "table-child")
                        {
                            isCorrect = CheckAnswer(question, userAnswerJsonDoc);
                            if (isCorrect)
                            {
                                // Cộng điểm dựa trên cách tính điểm mới
                                score += GetQuestionPointCount(question.CorrectAnswers);
                            }
                        }

                        userAnswersToSave.Add(new UserAnswer
                        {
                            TestAttemptId = attempt.Id,
                            QuestionId = questionId,
                            UserAnswerJson = userAnswerJsonDoc, // Lưu JsonDocument
                            IsCorrect = isCorrect
                        });
                    }
                }

                // Xử lý những câu hỏi người dùng không trả lời
                foreach (var question in allQuestions.Values.Where(q => q.QuestionType != "table-child"))
                {
                    if (userAnswersToSave.All(ua => ua.QuestionId != question.Id))
                    {
                        userAnswersToSave.Add(new UserAnswer
                        {
                            TestAttemptId = attempt.Id,
                            QuestionId = question.Id,
                            UserAnswerJson = JsonDocument.Parse("{}"), // Lưu JSON rỗng
                            IsCorrect = false
                        });
                    }
                }

                _context.UserAnswers.AddRange(userAnswersToSave);
                attempt.Score = score; // Gán điểm cuối cùng

                await _context.SaveChangesAsync(); // Lưu UserAnswers và cập nhật Score
                await transaction.CommitAsync();

                return attempt.Id;
            }
            catch (Exception ex) // Bắt lỗi cụ thể hơn để debug
            {
                Console.WriteLine($"SubmitTestAsync Error: {ex.ToString()}"); // Ghi log chi tiết lỗi
                await transaction.RollbackAsync();
                throw; // Ném lại lỗi để trả về 500
            }
        }

        public async Task<TestResultDto?> GetTestResultAsync(int attemptId)
        {
            var attempt = await _context.TestAttempts
                .AsNoTracking()
                .Include(a => a.Test)
                .Include(a => a.UserAnswers)
                    .ThenInclude(ua => ua.Question)
                        .ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(a => a.Id == attemptId);

            if (attempt == null) return null;

            return new TestResultDto
            {
                TestTitle = attempt.Test.Title,
                Score = attempt.Score,
                TotalQuestions = attempt.TotalQuestions,
                Questions = attempt.UserAnswers.Select(ua => new QuestionResultDto
                {
                    QuestionNumber = ua.Question.QuestionNumber,
                    Prompt = ua.Question.Prompt,

                    UserAnswer = ua.UserAnswerJson?.RootElement.Clone(),
                    CorrectAnswer = ua.Question.CorrectAnswers.RootElement.Clone(),

                    IsCorrect = ua.IsCorrect,
                    QuestionType = ua.Question.QuestionType,
                    Options = ua.Question.Options.Select(o => new QuestionOptionDto
                    {
                        Id = o.Id,
                        OptionLabel = o.OptionLabel,
                        OptionText = o.OptionText
                    }).ToList()
                }).OrderBy(q => q.QuestionNumber).ToList()
            };
        }

        // --- Helper Methods ---
        private bool CheckAnswer(Question question, JsonDocument userAnswerDoc)
        {
            if (question.CorrectAnswers == null) return false;

            JsonElement correctRoot = question.CorrectAnswers.RootElement;
            JsonElement userRoot = userAnswerDoc.RootElement;

            try
            {
                switch (question.QuestionType)
                {
                    case "fill-blank":
                    case "multiple-choice":
                        {
                            if (!correctRoot.TryGetProperty("answer", out var correctAnswerProp)) return false;
                            string correctAnswer = correctAnswerProp.GetString() ?? "";

                            // User answer đã là JsonDocument, lấy string trực tiếp
                            string userAnswer = userRoot.ValueKind == JsonValueKind.String ? userRoot.GetString() ?? "" : "";

                            // So sánh không phân biệt hoa thường và chấp nhận nhiều đáp án điền từ
                            return correctAnswer.Split(';')
                                .Any(ans => userAnswer.Trim().Equals(ans.Trim(), StringComparison.OrdinalIgnoreCase));
                        }

                    case "multiple-choice-multiple-answer":
                        {
                            // Deserialize từ JsonElement
                            var correctAnswerDto = JsonSerializer.Deserialize<CorrectAnswerDto>(correctRoot.GetRawText());
                            var correctSet = new HashSet<string>(correctAnswerDto?.Answers ?? new List<string>());

                            // User answer đã là JsonDocument, kiểm tra kind và deserialize
                            if (userRoot.ValueKind == JsonValueKind.Array)
                            {
                                var userSet = new HashSet<string>(userRoot.Deserialize<List<string>>() ?? new List<string>());
                                return correctSet.SetEquals(userSet);
                            }
                            return false;
                        }

                    case "table":
                        {
                            // Deserialize từ JsonElement
                            var correctAnswersDict = JsonSerializer.Deserialize<Dictionary<string, string>>(correctRoot.GetRawText());
                            if (correctAnswersDict == null || correctAnswersDict.Count == 0) return true; // Bảng không có đáp án?

                            // User answer đã là JsonDocument, deserialize
                            if (userRoot.ValueKind != JsonValueKind.Object) return false;
                            var userAnswersDict = JsonSerializer.Deserialize<Dictionary<string, string>>(userRoot.GetRawText());
                            if (userAnswersDict == null) return false;

                            // Kiểm tra tất cả các key trong đáp án đúng
                            return correctAnswersDict.All(correctPair =>
                                userAnswersDict.TryGetValue(correctPair.Key, out var userAnswerValue) &&
                                (userAnswerValue?.ToString() ?? "").Trim().Equals(correctPair.Value.Trim(), StringComparison.OrdinalIgnoreCase));
                        }
                    default:
                        return false;
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error checking answer ID {question.Id}: {ex.Message}. UserJSON: {userAnswerDoc.RootElement.GetRawText()}, CorrectJSON: {correctRoot.GetRawText()}");
                return false;
            }
            catch (Exception ex) // Bắt các lỗi khác
            {
                Console.WriteLine($"Unexpected error checking answer ID {question.Id}: {ex.ToString()}");
                return false;
            }
        }
        private JsonDocument GetUserAnswerForSaving(Question question, Dictionary<string, object> userAnswers)
        {
            try
            {
                if (question.QuestionType == "table")
                {
                    var tableAnswers = new Dictionary<string, string>();
                    if (question.TableData != null)
                    {
                        var tableRoot = question.TableData.RootElement;
                        if (tableRoot.TryGetProperty("tableData", out var rowsElement) && rowsElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var row in rowsElement.EnumerateArray())
                            {
                                if (row.ValueKind != JsonValueKind.Array) continue;
                                foreach (var cell in row.EnumerateArray())
                                {
                                    if (cell.TryGetProperty("isAnswer", out var isAnswerProp) && isAnswerProp.GetBoolean() &&
                                        cell.TryGetProperty("answerId", out var answerIdProp))
                                    {
                                        string answerId = answerIdProp.ValueKind switch
                                        {
                                            JsonValueKind.Number => answerIdProp.GetInt32().ToString(),
                                            JsonValueKind.String => answerIdProp.GetString() ?? "",
                                            _ => ""
                                        };
                                        if (!string.IsNullOrEmpty(answerId))
                                        {
                                            string userAnswerKey = $"q{question.Id}_{answerId}";
                                            if (userAnswers.TryGetValue(userAnswerKey, out var userAnswerObject))
                                            {
                                                tableAnswers[answerId] = userAnswerObject?.ToString() ?? "";
                                            }
                                            else
                                            {
                                                tableAnswers[answerId] = ""; // Lưu rỗng nếu user không điền
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return JsonDocument.Parse(JsonSerializer.Serialize(tableAnswers));
                }

                // Phần còn lại cho các loại câu hỏi khác giữ nguyên
                if (userAnswers.TryGetValue(question.Id.ToString(), out var answerObject))
                {
                    // Kiểm tra null trước khi Serialize
                    return JsonDocument.Parse(JsonSerializer.Serialize(answerObject ?? (object)""));
                }

                return JsonDocument.Parse("{}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserAnswerForSaving for question ID {question.Id}: {ex.Message}");
                return JsonDocument.Parse("{}");
            }
        }
        // Đếm điểm
        private int GetQuestionPointCount(JsonDocument? correctAnswersDoc)
        {
            if (correctAnswersDoc == null) return 1;

            try
            {
                var root = correctAnswersDoc.RootElement; // Làm việc trực tiếp với root element

                if (root.ValueKind == JsonValueKind.Object)
                {
                    // Nếu là dạng {"answer": "A"} -> 1 điểm
                    if (root.TryGetProperty("answer", out _)) return 1;

                    // Nếu là dạng {"answers": [...]} -> 1 điểm
                    if (root.TryGetProperty("answers", out _)) return 1;

                    // Đếm số key trong object (cho câu hỏi bảng)
                    // Trả về ít nhất 1 nếu có key nào đó, tránh trường hợp object rỗng {} trả về 0
                    int count = root.EnumerateObject().Count();
                    return count > 0 ? count : 1;
                }
                // Các trường hợp khác (Array, String, Number...) mặc định là 1 điểm
                return 1;
            }
            catch (JsonException) // Bắt lỗi parse cụ thể nếu cần
            {
                Console.WriteLine("Warning: Could not determine point count from CorrectAnswers JSON.");
                return 1; // Mặc định là 1 nếu JSON không hợp lệ
            }
            catch (Exception ex) // Bắt các lỗi khác
            {
                Console.WriteLine($"Unexpected error in GetQuestionPointCount: {ex.Message}");
                return 1;
            }
        }
        public async Task<IEnumerable<TestAttemptHistoryDto>> GetMyTestHistoryAsync()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                throw new UnauthorizedAccessException("Không thể xác định người dùng hiện tại.");
            }
            return await _context.TestAttempts
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .Include(a => a.Test)
                    .ThenInclude(t => t.TestType)
                .OrderByDescending(a => a.SubmittedAt)
                .Select(a => new TestAttemptHistoryDto
                {
                    Id = a.Id,
                    TestTitle = a.Test.Title,
                    Score = a.Score,
                    TotalQuestions = a.TotalQuestions,
                    SubmittedAt = a.SubmittedAt,
                    TestType = a.Test.TestType.Name
                })
                .ToListAsync();
        }

        private QuestionDto MapQuestionToDto(Question q)
        {
            return new QuestionDto
            {
                Id = q.Id,
                QuestionNumber = q.QuestionNumber,
                QuestionType = q.QuestionType,
                Prompt = q.Prompt,
                TableData = q.TableData,
                Options = q.Options.Select(o => new QuestionOptionDto
                {
                    Id = o.Id,
                    OptionLabel = o.OptionLabel,
                    OptionText = o.OptionText
                }).ToList()
            };
        }
    }
}