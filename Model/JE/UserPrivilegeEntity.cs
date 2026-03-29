using CF_User.Model.enums;

namespace CF_User.Model.JE
{
    public class UserPrivilegeEntity
    {
        public Guid UserId { get; set; }
        public AppUser User { get; set; }
        public UserPrivilege Privilege { get; set; }

    }
}
