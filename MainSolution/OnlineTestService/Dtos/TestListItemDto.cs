namespace OnlineTestService.Dtos
{
    public class TestListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public string? Description { get; set; }
        public string Duration { get; set; }
        public string Questions { get; set; }
    }
}
