using CF_User.Model.enums;
using CF_User.Model.JE;

namespace CF_User.Model
{
    public class AppUser
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public UserRole Role { get; set; }
        public ICollection<UserPrivilegeEntity> Privileges { get; private set; } =
            new List<UserPrivilegeEntity>();
        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public AppUser(string username, string email, string password)
        {
            Username = username;
            Email = email;
            PasswordHash = HashPassword(password);
        }

        public AppUser() { }

        public void SetPassword(string password)
        {
            PasswordHash = HashPassword(password);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public void SetEmail(string email)
        {
            Email = email;
        }

        public void SetUsername(string username)
        {
            Username = username;
        }
    }
}
