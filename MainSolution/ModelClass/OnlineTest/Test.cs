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

        [ForeignKey("TestTypeId")]
        public virtual TestType TestType { get; set; }

        public virtual ICollection<Passage> Passages { get; set; } = new List<Passage>();
    }
}
