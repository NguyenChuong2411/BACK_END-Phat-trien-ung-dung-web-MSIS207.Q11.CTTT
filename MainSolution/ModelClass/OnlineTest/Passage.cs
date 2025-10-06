using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.OnlineTest
{
    [Table("passages", Schema = "online_test")]
    public class Passage
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("test_id")]
        public int TestId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("display_order")]
        public int DisplayOrder { get; set; }

        [ForeignKey("TestId")]
        public virtual Test Test { get; set; }

        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    }
}
