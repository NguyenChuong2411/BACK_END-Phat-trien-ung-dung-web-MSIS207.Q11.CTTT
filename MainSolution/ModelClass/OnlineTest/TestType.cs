using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Net.Mime.MediaTypeNames;

namespace ModelClass.OnlineTest
{
    [Table("testtypes", Schema = "online_test")]
    public class TestType
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
