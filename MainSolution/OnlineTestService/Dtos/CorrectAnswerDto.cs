using System.Text.Json.Serialization;

namespace OnlineTestService.Dtos
{
    public class CorrectAnswerDto
    {
        [JsonPropertyName("answer")]
        public string? Answer { get; set; } // Dùng cho single-answer questions

        [JsonPropertyName("answers")]
        public List<string>? Answers { get; set; } // Dùng cho multiple-answer questions
    }
}
