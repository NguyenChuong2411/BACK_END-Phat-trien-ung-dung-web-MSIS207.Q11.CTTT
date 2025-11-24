using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.OnlineTest
{
    [Table("testattempts", Schema = "online_test")]
    public class TestAttempt
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("test_id")]
        public int TestId { get; set; }

        [Column("user_id")]
        public int? UserId { get; set; }

        [Column("score")]
        public int Score { get; set; }

        [Column("total_questions")]
        public int TotalQuestions { get; set; }

        [Column("submitted_at")]
        public DateTime SubmittedAt { get; set; }

        [ForeignKey("TestId")]
        public virtual Test Test { get; set; }
        public virtual ICollection<UserAnswer> UserAnswers { get; set; } = new List<UserAnswer>();
    }
}
