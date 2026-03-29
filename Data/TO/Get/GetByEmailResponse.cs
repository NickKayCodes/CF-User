using CF_User.Model.enums;

namespace CF_User.Data.TO.Get
{
    public class GetByEmailResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public UserRole Role { get; set; }
        public List<Privilege> Privileges { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
