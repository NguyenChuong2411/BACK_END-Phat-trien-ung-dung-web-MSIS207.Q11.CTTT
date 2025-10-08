using Microsoft.EntityFrameworkCore;
using ModelClass.connection;
using OnlineTestService.Dtos;

namespace OnlineTestService.Service.Impl
{
    public class OnlineTestImpl : IOnlineTest
    {
        private readonly OnlineTestDbContext _context;

        public OnlineTestImpl(OnlineTestDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TestListItemDto>> GetAllTestsAsync()
        {
            // Truy vấn và join với TestType để lấy tên loại đề thi
            return await _context.Tests
                .AsNoTracking() // Tối ưu hiệu năng cho truy vấn chỉ đọc
                .Include(t => t.TestType)
                .Select(t => new TestListItemDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Type = t.TestType.Name,
                    Description = t.Description,
                    Duration = $"{t.DurationMinutes} phút",
                    Questions = $"{t.TotalQuestions} câu hỏi"
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
    }
}
