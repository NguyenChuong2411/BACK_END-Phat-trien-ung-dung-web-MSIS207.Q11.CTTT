using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.OnlineTest
{
    [Table("speakingquestions", Schema = "online_test")]
    public class SpeakingQuestion
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("test_id")]
        public int TestId { get; set; }

        [Column("question_text")]
        public string QuestionText { get; set; }

        [Column("part_name")]
        public string? PartName { get; set; }

        [Column("preparation_time_seconds")]
        public int? PreparationTimeSeconds { get; set; }

        [Column("response_time_seconds")]
        public int? ResponseTimeSeconds { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [Column("audio_prompt_url")]
        public string? AudioPromptUrl { get; set; }

        [ForeignKey("TestId")]
        public virtual Test Test { get; set; }
    }
}
