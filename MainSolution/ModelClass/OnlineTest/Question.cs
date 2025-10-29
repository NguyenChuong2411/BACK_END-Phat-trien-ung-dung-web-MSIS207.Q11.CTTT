using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ModelClass.OnlineTest
{
    [Table("questions", Schema = "online_test")]
    public class Question
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("passage_id")]
        public int? PassageId { get; set; }

        [Column("question_number")]
        public int QuestionNumber { get; set; }

        [Column("question_type")]
        public string QuestionType { get; set; }

        [Column("prompt")]
        public string? Prompt { get; set; }

        [Column("table_data", TypeName = "jsonb")]
        public JsonDocument? TableData { get; set; }

        [Column("correct_answers", TypeName = "jsonb")]
        public JsonDocument CorrectAnswers { get; set; }

        [Column("question_group_id")]
        public int? QuestionGroupId { get; set; }

        [ForeignKey("PassageId")]
        public virtual Passage Passage { get; set; }

        [ForeignKey("QuestionGroupId")]
        public virtual QuestionGroup? QuestionGroup { get; set; }

        public virtual ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
    }
}
