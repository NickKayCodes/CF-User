using CF_User.Model.enums;

namespace CF_User.Data.TO.Create
{
    public class CreateUserResponse
    {
        public Guid Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
