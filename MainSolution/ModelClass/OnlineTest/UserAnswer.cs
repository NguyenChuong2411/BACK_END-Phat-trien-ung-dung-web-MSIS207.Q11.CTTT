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
    [Table("useranswers", Schema = "online_test")]
    public class UserAnswer
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("test_attempt_id")]
        public int TestAttemptId { get; set; }

        [Column("question_id")]
        public int QuestionId { get; set; }

        [Column("user_answer", TypeName = "jsonb")]
        public JsonDocument UserAnswerJson { get; set; }

        [Column("is_correct")]
        public bool IsCorrect { get; set; }

        [ForeignKey("TestAttemptId")]
        public virtual TestAttempt TestAttempt { get; set; }

        [ForeignKey("QuestionId")]
        public virtual Question Question { get; set; }
    }
}
