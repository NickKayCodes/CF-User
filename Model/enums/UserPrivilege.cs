namespace CF_User.Model.enums
{
    public enum UserPrivilege
    {
        //For ADMIN role
        FULL_ACCESS,

        //View
        VIEW_EVENT,
        VIEW_INVENTORY,
        VIEW_REPORT,
        VIEW_MENU,
        VIEW_EVENT_PLAN,
        VIEW_INVOICE,

        //Edit
        EDIT_REPORT,
        EDIT_EVENT,
        EDIT_INVENTORY,
        EDIT_MENU,
        EDIT_EVENT_PLAN,
        EDIT_INVOICE, //??

        //Manage / CREATE
        MANAGE_INVENTORY,
        MANAGE_STAFF,
        MANAGE_KITCHEN,
        UPDATE_MENU,
        CREATE_INVOICE,
        APPROVE_EVENT,
        CREATE_REPORT

    }
}
