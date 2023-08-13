using BugTracker.Authorization;

namespace BugTracker.Data.SeedData;

public static class SeedDefaultRoles
{
    public static async Task SeedAsync(RoleManager<IdentityRole> roleManager)
    {
        var defaultRoles = DefaultRoles.GenerateDefaultRolesList();

        foreach (var role in defaultRoles)
        {
            if (await roleManager.FindByNameAsync(role) is null)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                await AddPermissionClaimForAdminAsync(roleManager);
            }
        }
        
        //await AddPermissionClaimForAdminAsync(roleManager);
        //await AddPermissionClaimForProjectManagerAsync(roleManager);
        //await AddPermissionClaimForDeveloperAsync(roleManager);
        //await AddPermissionClaimForTesterAsync(roleManager);
        //await AddPermissionClaimForSubmitterAsync(roleManager);
    }

    // Add default permissions for admin
    public static async Task AddPermissionClaimForAdminAsync(this RoleManager<IdentityRole> roleManager)
    {
        var role = await roleManager.FindByNameAsync(DefaultRoles.Admin);
        var operations = AdminPermissions.Generate();

        await AddClaim(role, roleManager, operations);
    }

    // Add default permissions for project manager
    public static async Task AddPermissionClaimForProjectManagerAsync(this RoleManager<IdentityRole> roleManager)
    {
        var role = await roleManager.FindByNameAsync(DefaultRoles.ProjectManager);
        var operations = ProjectManagerPermissions.Generate();

        await AddClaim(role, roleManager, operations);
    }

    // Add default permissions for developer
    public static async Task AddPermissionClaimForDeveloperAsync(this RoleManager<IdentityRole> roleManager)
    {
        var role = await roleManager.FindByNameAsync(DefaultRoles.Developer);
        var operations = DeveloperPermissions.Generate();

        await AddClaim(role, roleManager, operations);
    }

    // Add default permissions for tester
    public static async Task AddPermissionClaimForTesterAsync(this RoleManager<IdentityRole> roleManager)
    {
        var role = await roleManager.FindByNameAsync(DefaultRoles.Tester);
        var operations = TesterPermissions.Generate();

        await AddClaim(role, roleManager, operations);
    }

    // Add default permissions for submitter
    public static async Task AddPermissionClaimForSubmitterAsync(this RoleManager<IdentityRole> roleManager)
    {
        var role = await roleManager.FindByNameAsync(DefaultRoles.Submitter);
        var operations = SubmitterPermissions.Generate();

        await AddClaim(role, roleManager, operations);
    }

    public static async Task AddClaim(
                                    IdentityRole role,
                                    RoleManager<IdentityRole> roleManager,
                                    List<string> operations)
    {
        var allClaims = await roleManager.GetClaimsAsync(role);
        foreach (var operation in operations)
        {
            if (!allClaims.Any(c => c.Type == "Permission" && c.Value == operation))
            {
                await roleManager.AddClaimAsync(role, new Claim("Permission", operation));
            }
        }
    }
}
