using System.Text.Json;

namespace OnlineTestService.Dtos
{
    public class TestResultDto
    {
        public string TestTitle { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public List<QuestionResultDto> Questions { get; set; } = new List<QuestionResultDto>();
    }

    public class QuestionResultDto
    {
        public int QuestionNumber { get; set; }
        public string? Prompt { get; set; }
        public JsonDocument? UserAnswer { get; set; }
        public JsonDocument CorrectAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public string QuestionType { get; set; }
        public List<QuestionOptionDto> Options { get; set; } = new List<QuestionOptionDto>();
    }
}
