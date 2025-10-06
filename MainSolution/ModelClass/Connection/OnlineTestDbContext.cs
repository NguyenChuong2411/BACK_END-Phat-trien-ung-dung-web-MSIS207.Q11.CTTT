using Microsoft.EntityFrameworkCore;
using ModelClass.OnlineTest;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestType>().ToTable("testtypes", "online_test");
            modelBuilder.Entity<Test>().ToTable("tests", "online_test");
            modelBuilder.Entity<Passage>().ToTable("passages", "online_test");
            modelBuilder.Entity<Question>().ToTable("questions", "online_test");
            modelBuilder.Entity<QuestionOption>().ToTable("questionoptions", "online_test");
        }
    }
}
