using Microsoft.EntityFrameworkCore;
using ModelClass.connection;
using ModelClass.OnlineTest;
using OnlineTestService.Dtos;
using System.Text.Json;
using System.Security.Claims;

namespace OnlineTestService.Service.Impl
{
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
            // Truy vấn và join với TestType để lấy tên loại đề thi
            return await _context.Tests
                .AsNoTracking()
                .Include(t => t.TestType)
                .Include(t => t.TestAttempts)
                .Select(t => new TestListItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Type = t.TestType.Name,
                    Description = t.Description,
                    Duration = $"{t.DurationMinutes} phút",
                    Questions = $"{t.TotalQuestions} câu hỏi",
                    Attempts = t.TestAttempts.Count()
                })
                .ToListAsync();
        }

        public async Task<FullTestDto?> GetTestDetailsByIdAsync(int testId)
        {
            // Eager loading tất cả dữ liệu liên quan trong 1 câu truy vấn duy nhất
            var test = await _context.Tests
                .AsNoTracking()
                .Include(t => t.Passages.OrderBy(p => p.DisplayOrder))
                    .ThenInclude(p => p.Questions.OrderBy(q => q.QuestionNumber))
                        .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null)
            {
                return null;
            }

            // Map thủ công từ Model sang DTO
            var fullTestDto = new FullTestDto
            {
                Id = test.Id,
                Title = test.Title,
                Passages = test.Passages.Select(p => new PassageDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    Questions = p.Questions.Select(q => new QuestionDto
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
                    }).ToList()
                }).ToList()
            };

            return fullTestDto;
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
                .FirstOrDefaultAsync(t => t.Id == testId);

            // Một bài thi Listening phải có file audio
            if (test == null || test.AudioFile == null)
            {
                return null;
            }

            // Xây dựng URL đầy đủ cho file audio
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var fullAudioUrl = $"{baseUrl}{test.AudioFile.StoragePath}";

            // Map từ Model sang DTO
            var listeningTestDto = new ListeningTestDto
            {
                Id = test.Id,
                Title = test.Title,
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
                        // Tái sử dụng logic map QuestionDto từ hàm GetTestDetailsByIdAsync
                        Questions = g.Questions.Select(q => new QuestionDto
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
                        }).ToList()
                    }).ToList()
                }).ToList()
            };

            return listeningTestDto;
        }
        // HÀM HELPER CHẤM ĐIỂM
        private bool CheckAnswer(Question question, Dictionary<string, object> userAnswers)
        {
            try
            {
                // Chuyển đổi object sang JsonElement để xử lý nhất quán
                if (!userAnswers.TryGetValue(question.Id.ToString(), out var userAnswerObject) && question.QuestionType != "table")
                {
                    return false;
                }

                switch (question.QuestionType)
                {
                    case "fill-blank":
                    case "multiple-choice":
                        {
                            var correctAnswerDto = JsonSerializer.Deserialize<CorrectAnswerDto>(question.CorrectAnswers.RootElement.GetRawText());
                            string userAnswer = userAnswerObject?.ToString() ?? "";
                            return userAnswer.Trim().Equals(correctAnswerDto?.Answer, StringComparison.OrdinalIgnoreCase);
                        }

                    case "multiple-choice-multiple-answer":
                        {
                            var correctAnswerDto = JsonSerializer.Deserialize<CorrectAnswerDto>(question.CorrectAnswers.RootElement.GetRawText());
                            var correctSet = new HashSet<string>(correctAnswerDto?.Answers ?? new List<string>());

                            if (userAnswerObject is JsonElement userAnswerJson && userAnswerJson.ValueKind == JsonValueKind.Array)
                            {
                                var userSet = new HashSet<string>(userAnswerJson.Deserialize<List<string>>() ?? new List<string>());
                                return correctSet.SetEquals(userSet);
                            }
                            return false;
                        }

                    case "table":
                        {
                            var correctAnswersDict = JsonSerializer.Deserialize<Dictionary<string, string>>(question.CorrectAnswers.RootElement.GetRawText());
                            if (correctAnswersDict == null || correctAnswersDict.Count == 0) return true;

                            foreach (var correctAnswerPair in correctAnswersDict)
                            {
                                string userAnswerKey = $"q{question.Id}_{correctAnswerPair.Key}";
                                if (!userAnswers.TryGetValue(userAnswerKey, out var userAnswerTableObject) ||
                                    !userAnswerTableObject.ToString().Trim().Equals(correctAnswerPair.Value, StringComparison.OrdinalIgnoreCase))
                                {
                                    return false; // Sai một ô là sai cả câu
                                }
                            }
                            return true;
                        }

                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        // HÀM HELPER LẤY CÂU TRẢ LỜI CỦA NGƯỜI DÙNG ĐỂ LƯU
        private JsonDocument GetUserAnswerForSaving(Question question, Dictionary<string, object> userAnswers)
        {
            if (question.QuestionType == "table")
            {
                var tableAnswers = new Dictionary<string, string>();
                var correctAnswersDict = JsonSerializer.Deserialize<Dictionary<string, string>>(question.CorrectAnswers.RootElement.GetRawText());

                foreach (var key in correctAnswersDict.Keys)
                {
                    string userAnswerKey = $"q{question.Id}_{key}";
                    if (userAnswers.TryGetValue(userAnswerKey, out var userAnswerObject))
                    {
                        tableAnswers[key] = userAnswerObject?.ToString() ?? "";
                    }
                }
                return JsonDocument.Parse(JsonSerializer.Serialize(tableAnswers));
            }

            if (userAnswers.TryGetValue(question.Id.ToString(), out var answerObject))
            {
                // Serialize lại object để đảm bảo định dạng JSON đúng
                return JsonDocument.Parse(JsonSerializer.Serialize(answerObject));
            }

            return JsonDocument.Parse("{}"); // Trả về JSON rỗng nếu không có câu trả lời
        }
        public async Task<int> SubmitTestAsync(TestSubmissionDto submission)
        {
            // Lấy bài test và tất cả các câu hỏi liên quan một cách trực tiếp
            var test = await _context.Tests
                .Include(t => t.Passages)
                    .ThenInclude(p => p.Questions)
                .Include(t => t.ListeningParts)
                    .ThenInclude(lp => lp.QuestionGroups)
                        .ThenInclude(qg => qg.Questions)
                .FirstOrDefaultAsync(t => t.Id == submission.TestId);

            if (test == null)
            {
                throw new Exception("Test not found");
            }

            // Tập hợp tất cả câu hỏi vào một Dictionary để dễ tra cứu
            var allQuestionsInTest = new Dictionary<int, Question>();

            // Lấy câu hỏi từ Reading Passages
            if (test.Passages != null)
            {
                foreach (var question in test.Passages.SelectMany(p => p.Questions))
                {
                    allQuestionsInTest[question.Id] = question;
                }
            }
            // Lấy câu hỏi từ Listening Parts
            if (test.ListeningParts != null)
            {
                foreach (var question in test.ListeningParts.SelectMany(lp => lp.QuestionGroups).SelectMany(qg => qg.Questions))
                {
                    allQuestionsInTest[question.Id] = question;
                }
            }

            // LẤY USER ID TỪ TOKEN
            var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                // Dòng này để phòng trường hợp endpoint không được bảo vệ bằng [Authorize]
                throw new UnauthorizedAccessException("Không thể xác thực người dùng từ token.");
            }
            var userId = int.Parse(userIdClaim.Value);

            int score = 0;
            var userAnswersToSave = new List<UserAnswer>();

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var testAttempt = new TestAttempt
                {
                    TestId = submission.TestId,
                    UserId = userId,
                    Score = 0,
                    TotalQuestions = allQuestionsInTest.Values.Count(q => q.QuestionType != "table-child"),
                    SubmittedAt = DateTime.UtcNow
                };
                _context.TestAttempts.Add(testAttempt);
                await _context.SaveChangesAsync();

                foreach (var question in allQuestionsInTest.Values)
                {
                    if (question.QuestionType == "table-child") continue;

                    bool isCorrect = CheckAnswer(question, submission.Answers);
                    if (isCorrect)
                    {
                        score++;
                    }

                    JsonDocument userAnswerJson = GetUserAnswerForSaving(question, submission.Answers);

                    userAnswersToSave.Add(new UserAnswer
                    {
                        TestAttemptId = testAttempt.Id,
                        QuestionId = question.Id,
                        UserAnswerJson = userAnswerJson,
                        IsCorrect = isCorrect
                    });
                }

                testAttempt.Score = score;
                _context.UserAnswers.AddRange(userAnswersToSave);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return testAttempt.Id;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine(ex.ToString());
                throw;
            }
        }

        // Phương thức để lấy kết quả
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

            var result = new TestResultDto
            {
                TestTitle = attempt.Test.Title,
                Score = attempt.Score,
                TotalQuestions = attempt.TotalQuestions,
                Questions = attempt.UserAnswers.Select(ua => new QuestionResultDto
                {
                    QuestionNumber = ua.Question.QuestionNumber,
                    Prompt = ua.Question.Prompt,
                    UserAnswer = ua.UserAnswerJson,
                    CorrectAnswer = ua.Question.CorrectAnswers,
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

            return result;
        }
    }
}
