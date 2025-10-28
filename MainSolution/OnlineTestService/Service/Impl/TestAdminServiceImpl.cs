using Microsoft.EntityFrameworkCore;
using ModelClass.connection;
using ModelClass.OnlineTest;
using OnlineTestService.Dtos;
using System.Text.Json;

namespace OnlineTestService.Service.Impl
{
    public class TestAdminServiceImpl : ITestAdminService
    {
        private readonly OnlineTestDbContext _context;

        public TestAdminServiceImpl(OnlineTestDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AdminTestListItemDto>> GetAllTestsForAdminAsync()
        {
            return await _context.Tests
                .AsNoTracking()
                .Include(t => t.TestType)
                .OrderBy(t => t.Id)
                .Select(t => new AdminTestListItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Description = t.Description,
                    TestType = t.TestType.Name,
                    QuestionCount = t.TotalQuestions,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                })
                .ToListAsync();
        }

        public async Task<ManageTestDto?> GetTestForEditAsync(int testId)
        {
            var test = await _context.Tests
                .AsNoTracking()
                .Include(t => t.Passages.OrderBy(p => p.DisplayOrder))
                    .ThenInclude(p => p.Questions.OrderBy(q => q.QuestionNumber))
                        .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
                .Include(t => t.ListeningParts.OrderBy(lp => lp.PartNumber))
                    .ThenInclude(lp => lp.QuestionGroups.OrderBy(qg => qg.DisplayOrder))
                        .ThenInclude(qg => qg.Questions.OrderBy(q => q.QuestionNumber))
                            .ThenInclude(q => q.Options.OrderBy(o => o.DisplayOrder))
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null) return null;

            // Map từ entity sang DTO để gửi về cho client
            return new ManageTestDto
            {
                Id = test.Id,
                Title = test.Title,
                Description = test.Description,
                DurationMinutes = test.DurationMinutes,
                TestTypeId = test.TestTypeId,
                AudioFileId = test.AudioFileId,
                Passages = test.Passages.Select(p => new ManagePassageDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Content = p.Content,
                    DisplayOrder = p.DisplayOrder,
                    Questions = p.Questions.Select(MapQuestionEntityToDto).ToList()
                }).ToList(),
                ListeningParts = test.ListeningParts.Select(lp => new ManageListeningPartDto
                {
                    Id = lp.Id,
                    PartNumber = lp.PartNumber,
                    Title = lp.Title,
                    QuestionGroups = lp.QuestionGroups.Select(qg => new ManageQuestionGroupDto
                    {
                        Id = qg.Id,
                        InstructionText = qg.InstructionText,
                        DisplayOrder = qg.DisplayOrder,
                        Questions = qg.Questions.Select(MapQuestionEntityToDto).ToList()
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<int> CreateTestAsync(ManageTestDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var test = new Test();
                MapDtoToEntity(test, dto);
                test.CreatedAt = DateTime.UtcNow;
                test.UpdatedAt = DateTime.UtcNow;
                _context.Tests.Add(test);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return test.Id;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateTestAsync(int testId, ManageTestDto dto)
        {
            var test = await _context.Tests
                .Include(t => t.Passages).ThenInclude(p => p.Questions).ThenInclude(q => q.Options)
                .Include(t => t.ListeningParts).ThenInclude(lp => lp.QuestionGroups).ThenInclude(qg => qg.Questions).ThenInclude(q => q.Options)
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null) return false;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Xóa hết các thành phần con cũ để đảm bảo dữ liệu mới được ghi đè chính xác
                _context.Passages.RemoveRange(test.Passages);
                _context.ListeningParts.RemoveRange(test.ListeningParts);
                await _context.SaveChangesAsync(); // Áp dụng thay đổi xóa

                MapDtoToEntity(test, dto);
                test.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteTestAsync(int testId)
        {
            var test = await _context.Tests.FindAsync(testId);
            if (test == null) return false;

            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
            return true;
        }
        private void MapDtoToEntity(Test test, ManageTestDto dto)
        {
            test.Title = dto.Title;
            test.Description = dto.Description;
            test.DurationMinutes = dto.DurationMinutes;
            test.TestTypeId = dto.TestTypeId;
            test.SkillTypeId = dto.SkillTypeId;
            test.AudioFileId = dto.AudioFileId;
            test.Passages.Clear();
            test.ListeningParts.Clear();

            int totalQuestionsCount = 0;

            foreach (var passageDto in dto.Passages)
            {
                var passage = new Passage { Title = passageDto.Title, Content = passageDto.Content, DisplayOrder = passageDto.DisplayOrder };
                foreach (var questionDto in passageDto.Questions)
                {
                    var question = MapQuestionDtoToEntity(questionDto);
                    passage.Questions.Add(question);
                    totalQuestionsCount += GetQuestionPointCount(question.CorrectAnswers); // Tính điểm
                }
                test.Passages.Add(passage);
            }

            foreach (var partDto in dto.ListeningParts)
            {
                var part = new ListeningPart { PartNumber = partDto.PartNumber, Title = partDto.Title };
                foreach (var groupDto in partDto.QuestionGroups)
                {
                    var group = new QuestionGroup { InstructionText = groupDto.InstructionText, DisplayOrder = groupDto.DisplayOrder };
                    foreach (var questionDto in groupDto.Questions)
                    {
                        var question = MapQuestionDtoToEntity(questionDto);
                        group.Questions.Add(question);
                        totalQuestionsCount += GetQuestionPointCount(question.CorrectAnswers); // Tính điểm
                    }
                    part.QuestionGroups.Add(group);
                }
                test.ListeningParts.Add(part);
            }
            test.TotalQuestions = totalQuestionsCount;
        }

        private Question MapQuestionDtoToEntity(ManageQuestionDto questionDto)
        {
            var question = new Question
            {
                QuestionNumber = questionDto.QuestionNumber,
                QuestionType = questionDto.QuestionType,
                Prompt = questionDto.Prompt,
                TableData = CloneJsonDocument(questionDto.TableData),
                CorrectAnswers = CloneAndParseJsonDocument(questionDto.CorrectAnswers) ?? JsonDocument.Parse("{}")
            };

            foreach (var optionDto in questionDto.Options)
            {
                question.Options.Add(new QuestionOption { OptionLabel = optionDto.OptionLabel, OptionText = optionDto.OptionText, DisplayOrder = optionDto.DisplayOrder });
            }
            return question;
        }

        private ManageQuestionDto MapQuestionEntityToDto(Question q)
        {
            return new ManageQuestionDto
            {
                Id = q.Id,
                QuestionNumber = q.QuestionNumber,
                QuestionType = q.QuestionType,
                Prompt = q.Prompt,
                TableData = q.TableData,
                CorrectAnswers = q.CorrectAnswers,
                Options = q.Options.Select(o => new ManageQuestionOptionDto
                {
                    Id = o.Id,
                    OptionLabel = o.OptionLabel,
                    OptionText = o.OptionText,
                    DisplayOrder = o.DisplayOrder
                }).ToList()
            };
        }
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
        // Dùng cho TableData (chỉ cần clone)
        private JsonDocument? CloneJsonDocument(JsonDocument? doc)
        {
            if (doc == null) return null;
            return JsonDocument.Parse(doc.RootElement.GetRawText());
        }

        // Dùng cho CorrectAnswers (fix lỗi double-serialization)
        private JsonDocument? CloneAndParseJsonDocument(JsonDocument? doc)
        {
            if (doc == null) return null;

            // 1. Kiểm tra xem RootElement có phải là STRING không
            if (doc.RootElement.ValueKind == JsonValueKind.String)
            {
                // 2. Lấy giá trị chuỗi bên trong
                string? stringValue = doc.RootElement.GetString();
                if (!string.IsNullOrEmpty(stringValue))
                {
                    try
                    {
                        // 3. Thử Parse chuỗi đó thành một JsonDocument MỚI
                        return JsonDocument.Parse(stringValue);
                    }
                    catch (JsonException)
                    {
                        // Nếu thất bại (ví dụ: nó chỉ là chuỗi "hello"),
                        // thì trả về tài liệu gốc (đã clone)
                        return JsonDocument.Parse(doc.RootElement.GetRawText());
                    }
                }
            }

            // 4. Nếu nó không phải là String (ví dụ: Object, Array...),
            // chỉ cần clone như bình thường
            return JsonDocument.Parse(doc.RootElement.GetRawText());
        }
    }
}