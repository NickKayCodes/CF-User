using CF_User.Model.Auth;
using CF_User.Model.enums;
using CF_User.Model.JE;

public class AppUser
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Username { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; set; }
    public ICollection<UserPrivilege> Privileges { get; private set; } = new List<UserPrivilege>();
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    public AppUser(string username, string email, string passwordHash)
    {
        Username = username;
        Email = email;
        PasswordHash = passwordHash; // <-- use already hashed password
    }

    public AppUser() { }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
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
