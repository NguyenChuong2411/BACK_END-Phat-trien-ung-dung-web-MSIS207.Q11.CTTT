using Microsoft.EntityFrameworkCore;
using ModelClass.test;

namespace ModelClass.connection
{
    public class OnlineTestDbContext : DbContext
    {
        public OnlineTestDbContext(DbContextOptions<OnlineTestDbContext> options)
            : base(options)
        {
        }

        public DbSet<Test> Tests { get; set; }
    }
}
