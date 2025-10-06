using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.OnlineTest
{
    [Table("questionoptions", Schema = "online_test")]
    public class QuestionOption
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("question_id")]
        public int QuestionId { get; set; }

        [Column("option_label")]
        public string OptionLabel { get; set; }

        [Column("option_text")]
        public string OptionText { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        // Mối quan hệ: Một Option thuộc về một Question
        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
    }
}
