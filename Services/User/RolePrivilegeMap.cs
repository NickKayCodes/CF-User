using CF_User.Model.enums;

namespace CF_User.Services.User
{
    public static class RolePrivilegeMap
    {
        public static readonly Dictionary<UserRole, List<Privilege>> Privileges = new()
        {
            {
                UserRole.ADMIN,
                new List<Privilege>
                {
                    Privilege.FULL_ACCESS
                }
            },
            {
                UserRole.MANAGER,
                new List<Privilege>
                {
                    Privilege.VIEW_EVENT,
                    Privilege.VIEW_INVENTORY,
                    Privilege.VIEW_REPORT,
                    Privilege.VIEW_MENU,
                    Privilege.VIEW_EVENT_PLAN,
                    Privilege.VIEW_INVOICE,
                    Privilege.EDIT_REPORT,
                    Privilege.EDIT_EVENT,
                    Privilege.EDIT_INVENTORY,
                    Privilege.EDIT_MENU,
                    Privilege.EDIT_EVENT_PLAN,
                    Privilege.MANAGE_INVENTORY,
                    Privilege.MANAGE_STAFF,
                    Privilege.MANAGE_KITCHEN,
                    Privilege.UPDATE_MENU,
                    Privilege.CREATE_INVOICE,
                    Privilege.APPROVE_EVENT,
                    Privilege.CREATE_REPORT
                }
            },
            {
                UserRole.KITCHEN_CHEF,
                new List<Privilege>
                {
                    Privilege.VIEW_MENU,
                    Privilege.VIEW_EVENT_PLAN,
                    Privilege.EDIT_MENU,
                    Privilege.MANAGE_KITCHEN,
                    Privilege.UPDATE_MENU
                }
            },
            {
                UserRole.EVENT_CHEF,
                new List<Privilege>
                {
                    Privilege.VIEW_EVENT,
                    Privilege.VIEW_MENU,
                    Privilege.VIEW_EVENT_PLAN,
                    Privilege.EDIT_EVENT,
                    Privilege.EDIT_EVENT_PLAN,
                    Privilege.APPROVE_EVENT
                }
            },
            {
                UserRole.SERVER,
                new List<Privilege>
                {
                    Privilege.VIEW_EVENT,
                    Privilege.VIEW_MENU,
                    Privilege.VIEW_EVENT_PLAN
                }
            },
            {
                UserRole.CAPTAIN,
                new List<Privilege>
                {
                    Privilege.VIEW_EVENT,
                    Privilege.VIEW_MENU,
                    Privilege.VIEW_EVENT_PLAN,
                    Privilege.EDIT_EVENT_PLAN
                }
            },
            {
                UserRole.SALES_PLANNER,
                new List<Privilege>
                {
                    Privilege.VIEW_EVENT,
                    Privilege.VIEW_MENU,
                    Privilege.VIEW_INVOICE,
                    Privilege.EDIT_EVENT,
                    Privilege.CREATE_INVOICE
                }
            },
            {
                UserRole.WAREHOUSE,
                new List<Privilege>
                {
                    Privilege.VIEW_INVENTORY,
                    Privilege.EDIT_INVENTORY,
                    Privilege.MANAGE_INVENTORY
                }
            }
        };
    }
}