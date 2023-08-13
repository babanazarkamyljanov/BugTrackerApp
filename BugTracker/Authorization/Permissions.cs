namespace BugTracker.Authorization;

public class Permissions
{
    public static class ProjectOperations
    {
        public static readonly string Create = "Permission.Project.Create";
        public static readonly string Read = "Permission.Project.Read";
        public static readonly string Update = "Permission.Project.Update";
        public static readonly string Delete = "Permission.Project.Delete";

        public static List<string> GenerateProjectOperations()
        {
            return new List<string>
            {
                Create,
                Read,
                Update,
                Delete
            };
        }
    }

    public static class BugOperations
    {
        public static readonly string Create = "Permission.Bug.Create";
        public static readonly string Read = "Permission.Bug.Read";
        public static readonly string Update = "Permission.Bug.Update";
        public static readonly string Delete = "Permission.Bug.Delete";

        public static List<string> GenerateBugOperations()
        {
            return new List<string>
            {
                Create,
                Read,
                Update,
                Delete
            };
        }
    }

    public static class AccountManageOperations
    {
        public static readonly string View = "Permission.AccountManage.View";
        public static readonly string AddPhoneNumber = "Permission.AccountManage.AddPhoneNumber";
        public static readonly string ChangeEmail = "Permission.AccountManage.ChangeEmail";
        public static readonly string ChangePassword = "Permission.AccountManage.ChangePassword";
        public static readonly string ChangeAvatar = "Permission.AccountManage.ChangeAvatar";
        public static readonly string Delete = "Permission.AccountManage.Delete";

        public static List<string> GenerateAccountManageOperations()
        {
            return new List<string>
            {
                View,
                AddPhoneNumber,
                ChangeEmail,
                ChangePassword,
                ChangeAvatar,
                Delete
            };
        }
    }

    public static class RoleManageOperations
    {
        public static readonly string Create = "Permission.Role.Create";
        public static readonly string Read = "Permission.Role.Read";
        public static readonly string Update = "Permission.Role.Update";
        public static readonly string Delete = "Permission.Role.Delete";

        public static List<string> GenerateRoleManageOperations()
        {
            return new List<string>
            {
                Create,
                Read,
                Update,
                Delete
            };
        }
    }

    public static class PermissionManageOperations
    {
        public static readonly string Read = "Permission.PermissionManage.Read";
        public static readonly string Update = "Permission.PermissionManage.Update";

        public static List<string> GeneratePermissionManageOperations()
        {
            return new List<string>
            {
                Read,
                Update
            };
        }
    }

    public static class OrganizationManageOperations
    {
        public static readonly string Edit = "Permission.OrganizationManage.Edit";
        public static readonly string CreateUser = "Permission.OrganizationManage.CreateUser";
        public static readonly string OrganizationUserManage = "Permission.OrganizationManage.OrganizationUserManage";

        public static List<string> GenerateOrganizationManageOperations()
        {
            return new List<string>
            {
                Edit,
                CreateUser,
                OrganizationUserManage
            };
        }
    }
}
