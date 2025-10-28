using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.OnlineTest
{
    [Table("tests", Schema = "online_test")]
    public class Test
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("description")]
        public string? Description { get; set; }

        [Column("duration_minutes")]
        public int DurationMinutes { get; set; }

        [Column("total_questions")]
        public int TotalQuestions { get; set; }

        [Column("test_type_id")]
        public int TestTypeId { get; set; }

        [Column("skill_type_id")]
        public int? SkillTypeId { get; set; }

        [Column("audio_file_id")]
        public int? AudioFileId { get; set; }
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [ForeignKey("TestTypeId")]
        public virtual TestType TestType { get; set; }

        [ForeignKey("SkillTypeId")]
        public virtual SkillType? SkillType { get; set; }

        [ForeignKey("AudioFileId")]
        public virtual AudioFile? AudioFile { get; set; }

        public virtual ICollection<Passage> Passages { get; set; } = new List<Passage>();
        public virtual ICollection<ListeningPart> ListeningParts { get; set; } = new List<ListeningPart>();
        public virtual ICollection<TestAttempt> TestAttempts { get; set; } = new List<TestAttempt>();
    }
}
