using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.OnlineTest
{
    [Table("writingtasks", Schema = "online_test")]
    public class WritingTask
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("test_id")]
        public int TestId { get; set; }

        [Column("task_type")]
        public string? TaskType { get; set; }

        [Column("prompt")]
        public string Prompt { get; set; }

        [Column("min_words")]
        public int? MinWords { get; set; }

        [Column("max_score")]
        public int? MaxScore { get; set; }

        [Column("duration_minutes")]
        public int? DurationMinutes { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [ForeignKey("TestId")]
        public virtual Test Test { get; set; }
    }
}
