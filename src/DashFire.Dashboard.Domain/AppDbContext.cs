using Microsoft.EntityFrameworkCore;

namespace DashFire.Dashboard.Domain
{
    public class AppDbContext : DbContext
    {
        private readonly DbContextOptions _options;

        public AppDbContext(DbContextOptions options) : base(options)
        {
            _options = options;
        }

        public AppDbContext Create()
        {
            return new AppDbContext(_options);
        }

        public DbSet<Job> Jobs
        {
            get;
            set;
        }

        public DbSet<Log> Logs
        {
            get;
            set;
        }
    }
}
