using System.Text.Json;

namespace OnlineTestService.Dtos
{
    public class FullTestDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? SkillTypeId { get; set; }
        public List<PassageDto> Passages { get; set; } = new List<PassageDto>();
    }

    public class PassageDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    }

    public class QuestionDto
    {
        public int Id { get; set; }
        public int QuestionNumber { get; set; }
        public string QuestionType { get; set; }
        public string? Prompt { get; set; }
        public JsonDocument? TableData { get; set; }
        public List<QuestionOptionDto> Options { get; set; } = new List<QuestionOptionDto>();
    }

    public class QuestionOptionDto
    {
        public int Id { get; set; }
        public string OptionLabel { get; set; }
        public string OptionText { get; set; }
    }
}
