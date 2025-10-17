using Microsoft.EntityFrameworkCore;
using ModelClass.connection;
using ModelClass.OnlineTest;
using OnlineTestService.Dtos;

namespace OnlineTestService.Service.Impl
{
    public class TestAdminServiceImpl : ITestAdminService
    {
        private readonly OnlineTestDbContext _context;

        public TestAdminServiceImpl(OnlineTestDbContext context)
        {
            _context = context;
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

            if (test == null)
            {
                return null;
            }

            // Map từ Model sang ManageTestDto
            var dto = new ManageTestDto
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
                    Questions = p.Questions.Select(MapQuestionToDto).ToList()
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
                        Questions = qg.Questions.Select(MapQuestionToDto).ToList()
                    }).ToList()
                }).ToList()
            };

            return dto;
        }
        private ManageQuestionDto MapQuestionToDto(Question q)
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
        public async Task<int> CreateTestAsync(ManageTestDto dto)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var test = new Test
                {
                    Title = dto.Title,
                    Description = dto.Description,
                    DurationMinutes = dto.DurationMinutes,
                    TestTypeId = dto.TestTypeId,
                    AudioFileId = dto.AudioFileId,
                    //CreatedAt = DateTime.UtcNow,
                    //UpdatedAt = DateTime.UtcNow
                };

                // Map Passages (Reading)
                foreach (var passageDto in dto.Passages)
                {
                    var passage = new Passage
                    {
                        Title = passageDto.Title,
                        Content = passageDto.Content,
                        DisplayOrder = passageDto.DisplayOrder
                    };

                    foreach (var questionDto in passageDto.Questions)
                    {
                        var question = new Question
                        {
                            QuestionNumber = questionDto.QuestionNumber,
                            QuestionType = questionDto.QuestionType,
                            Prompt = questionDto.Prompt,
                            TableData = questionDto.TableData,
                            CorrectAnswers = questionDto.CorrectAnswers
                        };

                        foreach (var optionDto in questionDto.Options)
                        {
                            question.Options.Add(new QuestionOption
                            {
                                OptionLabel = optionDto.OptionLabel,
                                OptionText = optionDto.OptionText,
                                DisplayOrder = optionDto.DisplayOrder
                            });
                        }
                        passage.Questions.Add(question);
                    }
                    test.Passages.Add(passage);
                }

                // Map Listening Parts
                foreach (var partDto in dto.ListeningParts)
                {
                    var part = new ListeningPart
                    {
                        PartNumber = partDto.PartNumber,
                        Title = partDto.Title
                    };

                    foreach (var groupDto in partDto.QuestionGroups)
                    {
                        var group = new QuestionGroup
                        {
                            InstructionText = groupDto.InstructionText,
                            DisplayOrder = groupDto.DisplayOrder
                        };

                        foreach (var questionDto in groupDto.Questions)
                        {
                            var question = new Question
                            {
                                QuestionNumber = questionDto.QuestionNumber,
                                QuestionType = questionDto.QuestionType,
                                Prompt = questionDto.Prompt,
                                TableData = questionDto.TableData,
                                CorrectAnswers = questionDto.CorrectAnswers
                            };

                            foreach (var optionDto in questionDto.Options)
                            {
                                question.Options.Add(new QuestionOption
                                {
                                    OptionLabel = optionDto.OptionLabel,
                                    OptionText = optionDto.OptionText,
                                    DisplayOrder = optionDto.DisplayOrder
                                });
                            }
                            group.Questions.Add(question);
                        }
                        part.QuestionGroups.Add(group);
                    }
                    test.ListeningParts.Add(part);
                }

                // Tính tổng số câu hỏi
                test.TotalQuestions = test.Passages.SelectMany(p => p.Questions).Count() + test.ListeningParts.SelectMany(lp => lp.QuestionGroups).SelectMany(qg => qg.Questions).Count();

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
                // Xóa hết các thành phần con cũ
                _context.Passages.RemoveRange(test.Passages);
                _context.ListeningParts.RemoveRange(test.ListeningParts);
                await _context.SaveChangesAsync();

                // Cập nhật thông tin chính của Test
                test.Title = dto.Title;
                test.Description = dto.Description;
                test.DurationMinutes = dto.DurationMinutes;
                test.TestTypeId = dto.TestTypeId;
                test.AudioFileId = dto.AudioFileId;
                test.UpdatedAt = DateTime.UtcNow;

                // Map Passages (Reading)
                foreach (var passageDto in dto.Passages)
                {
                    var passage = new Passage
                    {
                        Title = passageDto.Title,
                        Content = passageDto.Content,
                        DisplayOrder = passageDto.DisplayOrder
                    };

                    foreach (var questionDto in passageDto.Questions)
                    {
                        var question = new Question
                        {
                            QuestionNumber = questionDto.QuestionNumber,
                            QuestionType = questionDto.QuestionType,
                            Prompt = questionDto.Prompt,
                            TableData = questionDto.TableData,
                            CorrectAnswers = questionDto.CorrectAnswers
                        };

                        foreach (var optionDto in questionDto.Options)
                        {
                            question.Options.Add(new QuestionOption
                            {
                                OptionLabel = optionDto.OptionLabel,
                                OptionText = optionDto.OptionText,
                                DisplayOrder = optionDto.DisplayOrder
                            });
                        }
                        passage.Questions.Add(question);
                    }
                    test.Passages.Add(passage);
                }

                // Map Listening Parts
                foreach (var partDto in dto.ListeningParts)
                {
                    var part = new ListeningPart
                    {
                        PartNumber = partDto.PartNumber,
                        Title = partDto.Title
                    };

                    foreach (var groupDto in partDto.QuestionGroups)
                    {
                        var group = new QuestionGroup
                        {
                            InstructionText = groupDto.InstructionText,
                            DisplayOrder = groupDto.DisplayOrder
                        };

                        foreach (var questionDto in groupDto.Questions)
                        {
                            var question = new Question
                            {
                                QuestionNumber = questionDto.QuestionNumber,
                                QuestionType = questionDto.QuestionType,
                                Prompt = questionDto.Prompt,
                                TableData = questionDto.TableData,
                                CorrectAnswers = questionDto.CorrectAnswers
                            };

                            foreach (var optionDto in questionDto.Options)
                            {
                                question.Options.Add(new QuestionOption
                                {
                                    OptionLabel = optionDto.OptionLabel,
                                    OptionText = optionDto.OptionText,
                                    DisplayOrder = optionDto.DisplayOrder
                                });
                            }
                            group.Questions.Add(question);
                        }
                        part.QuestionGroups.Add(group);
                    }
                    test.ListeningParts.Add(part);
                }
                test.TotalQuestions = dto.Passages.SelectMany(p => p.Questions).Count() + dto.ListeningParts.SelectMany(lp => lp.QuestionGroups).SelectMany(qg => qg.Questions).Count(q => q.QuestionType != "table-child");
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
    }
}
