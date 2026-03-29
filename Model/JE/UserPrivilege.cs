using CF_User.Model.enums;

namespace CF_User.Model.JE
{
    public class UserPrivilege
    {
        public Guid UserId { get; set; }
        public AppUser User { get; set; }
        public Privilege Privilege { get; set; }

    }
}
