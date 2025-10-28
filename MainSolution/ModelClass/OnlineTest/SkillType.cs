using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.OnlineTest
{
    [Table("skilltypes", Schema = "online_test")]
    public class SkillType
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("slug")]
        public string Slug { get; set; }

        public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
    }
}
