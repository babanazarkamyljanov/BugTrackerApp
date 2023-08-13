namespace BugTracker.Authorization;

public static class DefaultRoles
{
    public static readonly string Admin = "Admin";
    public static readonly string ProjectManager = "Project Manager";
    public static readonly string Developer = "Developer";
    public static readonly string Tester = "Tester";
    public static readonly string Submitter = "Submitter";

    public static List<string> GenerateDefaultRolesList() =>
        new() { Admin, ProjectManager, Developer, Tester, Submitter };
}

public static class AdminPermissions
{
    public static List<string> Generate()
    {
        List<string> operations = new();

        foreach (var operation in Permissions.ProjectOperations.GenerateProjectOperations())
        {
            operations.Add(operation);
        }

        foreach (var operation in Permissions.BugOperations.GenerateBugOperations())
        {
            operations.Add(operation);
        }

        foreach (var operation in Permissions.AccountManageOperations.GenerateAccountManageOperations())
        {
            operations.Add(operation);
        }

        foreach (var operation in Permissions.RoleManageOperations.GenerateRoleManageOperations())
        {
            operations.Add(operation);
        }

        foreach (var operation in Permissions.PermissionManageOperations.GeneratePermissionManageOperations())
        {
            operations.Add(operation);
        }

        foreach (var operation in Permissions.OrganizationManageOperations.GenerateOrganizationManageOperations())
        {
            operations.Add(operation);
        }

        return operations;
    }
}

public static class ProjectManagerPermissions
{
    public static List<string> Generate()
    {
        List<string> operations = new();

        foreach (var operation in Permissions.ProjectOperations.GenerateProjectOperations())
        {
            operations.Add(operation);
        }

        foreach (var operation in Permissions.BugOperations.GenerateBugOperations())
        {
            operations.Add(operation);
        }

        operations.Add(Permissions.AccountManageOperations.ChangeAvatar);
        operations.Add(Permissions.AccountManageOperations.View);

        return operations;
    }
}

public static class DeveloperPermissions
{
    public static List<string> Generate()
    {
        List<string> operations = new()
        {
            Permissions.ProjectOperations.Read,

            Permissions.BugOperations.Read,
            Permissions.BugOperations.Update,

            Permissions.AccountManageOperations.ChangeAvatar,
            Permissions.AccountManageOperations.View
        };

        return operations;
    }
}

public static class TesterPermissions
{
    public static List<string> Generate()
    {
        List<string> operations = new()
        {
            Permissions.ProjectOperations.Read,

            Permissions.BugOperations.Create,
            Permissions.BugOperations.Read,
            Permissions.BugOperations.Update,

            Permissions.AccountManageOperations.ChangeAvatar,
            Permissions.AccountManageOperations.View
        };

        return operations;
    }
}

public static class SubmitterPermissions
{
    public static List<string> Generate()
    {
        List<string> operations = new()
        {
            Permissions.ProjectOperations.Read,
            Permissions.AccountManageOperations.ChangeAvatar,
            Permissions.AccountManageOperations.View
        };

        foreach (var operation in Permissions.BugOperations.GenerateBugOperations())
        {
            operations.Add(operation);
        }

        return operations;
    }
}
