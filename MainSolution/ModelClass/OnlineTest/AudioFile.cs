using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelClass.OnlineTest
{
    [Table("audiofiles", Schema = "online_test")]
    public class AudioFile
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("file_name")]
        public string FileName { get; set; }

        [Column("storage_path")]
        public string StoragePath { get; set; }

        [Column("duration_seconds")]
        public int? DurationSeconds { get; set; }
        [Column("uploaded_at")]
        public DateTime UploadedAt { get; set; }
    }
}
