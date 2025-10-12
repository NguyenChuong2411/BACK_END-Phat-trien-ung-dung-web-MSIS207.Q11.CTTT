using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.OnlineTest
{
    [Table("questiongroups", Schema = "online_test")]
    public class QuestionGroup
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("part_id")]
        public int PartId { get; set; }

        [Column("instruction_text")]
        public string? InstructionText { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [ForeignKey("PartId")]
        public virtual ListeningPart ListeningPart { get; set; }
        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
