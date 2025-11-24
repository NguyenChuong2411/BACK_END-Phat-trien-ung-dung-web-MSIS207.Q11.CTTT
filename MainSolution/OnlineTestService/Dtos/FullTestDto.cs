using System.Text.Json;

namespace OnlineTestService.Dtos
{
    public class FullTestDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? SkillTypeId { get; set; }
        public List<WritingTaskDto> WritingTasks { get; set; } = new List<WritingTaskDto>();
        public List<SpeakingQuestionDto> SpeakingQuestions { get; set; } = new List<SpeakingQuestionDto>();
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
    
    public class WritingTaskDto
    {
        public int Id { get; set; }
        public string TaskType { get; set; }
        public string Prompt { get; set; }
        public int DurationMinutes { get; set; }
        public int MinWords { get; set; }
    }

    public class SpeakingQuestionDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; }
        public string PartName { get; set; }
        public int PreparationTime { get; set; }
        public int ResponseTime { get; set; }
    }
}
