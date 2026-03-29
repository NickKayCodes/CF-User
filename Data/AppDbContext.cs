using CF_User.Model;
using CF_User.Model.enums;
using CF_User.Model.JE;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CF_User.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<AppUser> Users => Set<AppUser>();

        public DbSet<UserPrivilege> UserPrivileges => Set<UserPrivilege>();

        // configure the model for db entity framework
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ensure username is unique
            //Java equivalent would be: @Entity @Table(uniqueConstraints = @UniqueConstraint(columnNames = "username"))
            modelBuilder.Entity<AppUser>().HasIndex(u => u.Username).IsUnique();

            /**ensure email is unique as well
                Java equivalent would be:
                @Entity
                @Table(uniqueConstraints = @UniqueConstraint(columnNames = "email"))

             **/
            modelBuilder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();

            // configure enum to string conversion for UserRole in AppUser
            modelBuilder.Entity<AppUser>().Property(u => u.Role).HasConversion<string>();

            // configure the UserPrivilegeEntity with composite key and relationships
            modelBuilder.Entity<UserPrivilege>().ToTable("UserPrivileges").HasKey(p => new { p.UserId, p.Privilege });

            // configure enum to string conversion for UserPrivilege 
            modelBuilder
                .Entity<UserPrivilege>()
                .Property(p => p.Privilege)
                .HasConversion<string>();

            // configure the relationship between AppUser and UserPrivilegeEntity
            modelBuilder
                .Entity<UserPrivilege>()
                .HasOne(p => p.User)
                .WithMany(u => u.Privileges)
                .HasForeignKey(p => p.UserId);


        }
    }
}
