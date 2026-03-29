using CF_User.Model.enums;

namespace CF_User.Data.TO.Update
{
    public class UpdateUserRequest
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public List<Privilege>? Privileges { get; set; }
    }
}
