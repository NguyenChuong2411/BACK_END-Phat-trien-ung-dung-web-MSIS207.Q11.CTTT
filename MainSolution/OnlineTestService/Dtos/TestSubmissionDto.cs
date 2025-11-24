using System.Text.Json;

namespace OnlineTestService.Dtos
{
    public class TestSubmissionDto
    {
        public int TestId { get; set; }
        public Dictionary<string, object> Answers { get; set; }
    }
}
