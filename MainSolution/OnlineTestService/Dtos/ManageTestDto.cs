using System.Text.Json;

namespace OnlineTestService.Dtos
{
    // DTO chính để quản lý (Thêm/Sửa) một bài test
    public class ManageTestDto
    {
        public int Id { get; set; } // Sẽ là 0 khi tạo mới
        public string Title { get; set; }
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public int TestTypeId { get; set; }
        public int? SkillTypeId { get; set; }
        public int? AudioFileId { get; set; } // Dành cho bài thi Listening
        public string? AudioFilePath { get; set; }

        // Dùng một trong hai, tùy thuộc vào loại đề thi
        public List<ManagePassageDto> Passages { get; set; } = new();
        public List<ManageListeningPartDto> ListeningParts { get; set; } = new();
    }

    // --- DTOs cho Reading Test ---
    public class ManagePassageDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int DisplayOrder { get; set; }
        public List<ManageQuestionDto> Questions { get; set; } = new();
    }

    // --- DTOs cho Listening Test ---
    public class ManageListeningPartDto
    {
        public int Id { get; set; }
        public int PartNumber { get; set; }
        public string Title { get; set; }
        public List<ManageQuestionGroupDto> QuestionGroups { get; set; } = new();
    }

    public class ManageQuestionGroupDto
    {
        public int Id { get; set; }
        public string? InstructionText { get; set; }
        public int DisplayOrder { get; set; }
        public List<ManageQuestionDto> Questions { get; set; } = new();
    }

    // --- DTOs chung cho Câu hỏi và Lựa chọn ---
    public class ManageQuestionDto
    {
        public int Id { get; set; }
        public int QuestionNumber { get; set; }
        public string QuestionType { get; set; }
        public string? Prompt { get; set; }
        public JsonDocument? TableData { get; set; }
        public JsonDocument CorrectAnswers { get; set; }
        public List<ManageQuestionOptionDto> Options { get; set; } = new();
    }

    public class ManageQuestionOptionDto
    {
        public int Id { get; set; }
        public string OptionLabel { get; set; }
        public string OptionText { get; set; }
        public int DisplayOrder { get; set; }
    }
}
