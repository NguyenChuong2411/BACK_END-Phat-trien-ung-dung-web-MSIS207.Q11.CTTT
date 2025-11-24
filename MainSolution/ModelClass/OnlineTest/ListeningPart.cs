using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.OnlineTest
{
    [Table("listeningparts", Schema = "online_test")]
    public class ListeningPart
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("test_id")]
        public int TestId { get; set; }

        [Column("part_number")]
        public int PartNumber { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [ForeignKey("TestId")]
        public virtual Test Test { get; set; }
        public virtual ICollection<QuestionGroup> QuestionGroups { get; set; } = new List<QuestionGroup>();
    }
}
