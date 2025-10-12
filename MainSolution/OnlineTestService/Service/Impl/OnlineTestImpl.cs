using Microsoft.EntityFrameworkCore;
using ModelClass.connection;
using OnlineTestService.Dtos;

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
    }
}
