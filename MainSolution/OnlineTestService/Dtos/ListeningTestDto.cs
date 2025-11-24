namespace OnlineTestService.Dtos
{
    public class ListeningTestDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int? SkillTypeId { get; set; }
        public string AudioUrl { get; set; }
        public List<ListeningPartDto> Parts { get; set; } = new List<ListeningPartDto>();
        public List<PassageDto> Passages { get; set; } = new List<PassageDto>();
    }
    public class ListeningPartDto
    {
        public int Id { get; set; }
        public int PartNumber { get; set; }
        public string Title { get; set; }
        public List<QuestionGroupDto> QuestionGroups { get; set; } = new List<QuestionGroupDto>();
    }
    public class QuestionGroupDto
    {
        public int Id { get; set; }
        public string? InstructionText { get; set; }
        public List<QuestionDto> Questions { get; set; } = new List<QuestionDto>();
    }
}
