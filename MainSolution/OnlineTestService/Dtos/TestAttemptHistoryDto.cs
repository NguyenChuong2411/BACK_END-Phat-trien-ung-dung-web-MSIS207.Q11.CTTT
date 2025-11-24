namespace OnlineTestService.Dtos
{
    public class TestAttemptHistoryDto
    {
        public int Id { get; set; }
        public string TestTitle { get; set; }
        public int Score { get; set; }
        public int TotalQuestions { get; set; }
        public DateTime SubmittedAt { get; set; }
        public string TestType { get; set; }
    }
}
