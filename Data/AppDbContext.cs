using CF_User.Model;
using Microsoft.EntityFrameworkCore;

namespace CF_User.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<AppUser> Users => Set<AppUser>();
    }
}

