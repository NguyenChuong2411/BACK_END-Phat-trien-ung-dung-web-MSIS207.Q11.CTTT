namespace OnlineTestService.Dtos
{
    public class AdminTestListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string TestType { get; set; }
        public int QuestionCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
