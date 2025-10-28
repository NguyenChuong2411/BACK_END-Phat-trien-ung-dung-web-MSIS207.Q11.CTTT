using Microsoft.EntityFrameworkCore;
using ModelClass.OnlineTest;
using ModelClass.UserInfo;
using System.Text.Json;

namespace ModelClass.connection
{
    public class OnlineTestDbContext : DbContext
    {
        public OnlineTestDbContext(DbContextOptions<OnlineTestDbContext> options) : base(options)
        {
        }

        // Khai báo các bảng mà EF Core sẽ quản lý
        public DbSet<TestType> TestTypes { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<Passage> Passages { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionOption> QuestionOptions { get; set; }
        public DbSet<AudioFile> AudioFiles { get; set; }
        public DbSet<ListeningPart> ListeningParts { get; set; }
        public DbSet<QuestionGroup> QuestionGroups { get; set; }
        public DbSet<TestAttempt> TestAttempts { get; set; }
        public DbSet<UserAnswer> UserAnswers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<SkillType> SkillTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Cấu hình cho CorrectAnswers
            modelBuilder.Entity<Question>()
                .Property(q => q.CorrectAnswers)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => v.RootElement.GetRawText(),
                    v => JsonDocument.Parse(string.IsNullOrEmpty(v) ? "{}" : v, new JsonDocumentOptions())
                );

            // Cấu hình cho TableData
            modelBuilder.Entity<Question>()
               .Property(q => q.TableData)
               .HasColumnType("jsonb")
               .HasConversion(
                   v => v != null ? v.RootElement.GetRawText() : null,
                   v => !string.IsNullOrEmpty(v) ? JsonDocument.Parse(v, new JsonDocumentOptions()) : null
               );
            // Converter cho UserAnswer.UserAnswerJson
            modelBuilder.Entity<UserAnswer>()
                .Property(ua => ua.UserAnswerJson)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => v != null ? v.RootElement.GetRawText() : null,
                    v => !string.IsNullOrEmpty(v) ? JsonDocument.Parse(v, new JsonDocumentOptions()) : null
                );
            modelBuilder.Entity<TestType>().ToTable("testtypes", "online_test");
            modelBuilder.Entity<Test>().ToTable("tests", "online_test");
            modelBuilder.Entity<Passage>().ToTable("passages", "online_test");
            modelBuilder.Entity<Question>().ToTable("questions", "online_test");
            modelBuilder.Entity<QuestionOption>().ToTable("questionoptions", "online_test");
            modelBuilder.Entity<User>().ToTable("users", "user_info");
            modelBuilder.Entity<AudioFile>().ToTable("audiofiles", "online_test");
            modelBuilder.Entity<ListeningPart>().ToTable("listeningparts", "online_test");
            modelBuilder.Entity<QuestionGroup>().ToTable("questiongroups", "online_test");
            modelBuilder.Entity<TestAttempt>().ToTable("testattempts", "online_test");
            modelBuilder.Entity<UserAnswer>().ToTable("useranswers", "online_test");
            modelBuilder.Entity<SkillType>().ToTable("skilltypes", "online_test");
        }
    }
}
