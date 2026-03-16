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

        // configure the model for db entity framework
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ensure username is unique
            //Java equivalent would be: @Entity @Table(uniqueConstraints = @UniqueConstraint(columnNames = "username"))
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            /**ensure email is unique as well
                Java equivalent would be:
                @Entity
                @Table(uniqueConstraints = @UniqueConstraint(columnNames = "email"))

             **/
            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // store the enum as string in the database
            // Java equivalent would be: @Enumerated(EnumType.STRING)
            modelBuilder.Entity<AppUser>()
                .Property(u => u.Role)
                .HasConversion<string>();

        }
    }
}

