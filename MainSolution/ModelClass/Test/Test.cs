using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModelClass.test
{
    [Table("test", Schema = "public")] // Schema = public cho Postgre
    public class Test
    {
        [Key]
        public int test_id { get; set; }

        public string? title { get; set; }
        public string? description { get; set; }
        public DateTime? created_at { get; set; }
    }
}
