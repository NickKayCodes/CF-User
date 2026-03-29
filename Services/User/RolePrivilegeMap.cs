using CF_User.Model.enums;

namespace CF_User.Services.User
{
    public static class RolePrivilegeMap
    {
        public static readonly Dictionary<UserRole, List<UserPrivilege>> Privileges = new()
        {
            {
                UserRole.ADMIN,
                new List<UserPrivilege>
                {
                    UserPrivilege.FULL_ACCESS
                }
            },
            {
                UserRole.MANAGER,
                new List<UserPrivilege>
                {
                    UserPrivilege.VIEW_EVENT,
                    UserPrivilege.VIEW_INVENTORY,
                    UserPrivilege.VIEW_REPORT,
                    UserPrivilege.VIEW_MENU,
                    UserPrivilege.VIEW_EVENT_PLAN,
                    UserPrivilege.VIEW_INVOICE,
                    UserPrivilege.EDIT_REPORT,
                    UserPrivilege.EDIT_EVENT,
                    UserPrivilege.EDIT_INVENTORY,
                    UserPrivilege.EDIT_MENU,
                    UserPrivilege.EDIT_EVENT_PLAN,
                    UserPrivilege.MANAGE_INVENTORY,
                    UserPrivilege.MANAGE_STAFF,
                    UserPrivilege.MANAGE_KITCHEN,
                    UserPrivilege.UPDATE_MENU,
                    UserPrivilege.CREATE_INVOICE,
                    UserPrivilege.APPROVE_EVENT,
                    UserPrivilege.CREATE_REPORT
                }
            },
            {
                UserRole.KITCHEN_CHEF,
                new List<UserPrivilege>
                {
                    UserPrivilege.VIEW_MENU,
                    UserPrivilege.VIEW_EVENT_PLAN,
                    UserPrivilege.EDIT_MENU,
                    UserPrivilege.MANAGE_KITCHEN,
                    UserPrivilege.UPDATE_MENU
                }
            },
            {
                UserRole.EVENT_CHEF,
                new List<UserPrivilege>
                {
                    UserPrivilege.VIEW_EVENT,
                    UserPrivilege.VIEW_MENU,
                    UserPrivilege.VIEW_EVENT_PLAN,
                    UserPrivilege.EDIT_EVENT,
                    UserPrivilege.EDIT_EVENT_PLAN,
                    UserPrivilege.APPROVE_EVENT
                }
            },
            {
                UserRole.SERVER,
                new List<UserPrivilege>
                {
                    UserPrivilege.VIEW_EVENT,
                    UserPrivilege.VIEW_MENU,
                    UserPrivilege.VIEW_EVENT_PLAN
                }
            },
            {
                UserRole.CAPTAIN,
                new List<UserPrivilege>
                {
                    UserPrivilege.VIEW_EVENT,
                    UserPrivilege.VIEW_MENU,
                    UserPrivilege.VIEW_EVENT_PLAN,
                    UserPrivilege.EDIT_EVENT_PLAN
                }
            },
            {
                UserRole.SALES_PLANNER,
                new List<UserPrivilege>
                {
                    UserPrivilege.VIEW_EVENT,
                    UserPrivilege.VIEW_MENU,
                    UserPrivilege.VIEW_INVOICE,
                    UserPrivilege.EDIT_EVENT,
                    UserPrivilege.CREATE_INVOICE
                }
            },
            {
                UserRole.WAREHOUSE,
                new List<UserPrivilege>
                {
                    UserPrivilege.VIEW_INVENTORY,
                    UserPrivilege.EDIT_INVENTORY,
                    UserPrivilege.MANAGE_INVENTORY
                }
            }
        };
    }
}