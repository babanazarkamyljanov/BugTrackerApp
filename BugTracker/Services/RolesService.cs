using BugTracker.Authorization;
using BugTracker.Interfaces;

namespace BugTracker.Services;

public class RolesService : IRolesService
{
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RolesService> _logger;

    public RolesService(RoleManager<Role> roleManager,
        UserManager<User> userManager,
        ILogger<RolesService> logger)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task CreateDefaultRoles(User user, Organization organization)
    {
        List<string> defaultRoles = DefaultRoles.GenerateDefaultRolesList();
        foreach (var roleName in defaultRoles)
        {
            string uniqueName = roleName + "_" + organization.Name;
            if (!await _roleManager.Roles
                .Where(r => r.Name == uniqueName && r.OrganizationId == organization.Id)
                .AnyAsync())
            {
                try
                {
                    await _roleManager.CreateAsync(new Role() { Name = uniqueName, OrganizationId = organization.Id });
                }
                catch
                {
                    throw new ArgumentException("Create default roles was failed for organization", nameof(organization));
                }
            }
        }

        List<Role> orgRoles = await _roleManager.Roles
            .Where(r => r.OrganizationId == organization.Id)
            .ToListAsync();
        foreach (var role in orgRoles)
        {
            List<string> operations = new();
            if (role.Name == DefaultRoles.Admin + "_" + organization.Name)
            {
                operations = AdminPermissions.Generate();
            }
            else if (role.Name == DefaultRoles.ProjectManager + "_" + organization.Name)
            {
                operations = ProjectManagerPermissions.Generate();
            }
            else if (role.Name == DefaultRoles.Developer + "_" + organization.Name)
            {
                operations = DeveloperPermissions.Generate();
            }
            else if (role.Name == DefaultRoles.Tester + "_" + organization.Name)
            {
                operations = TesterPermissions.Generate();
            }
            else if (role.Name == DefaultRoles.Submitter + "_" + organization.Name)
            {
                operations = SubmitterPermissions.Generate();
            }

            await AddClaim(role, operations);
        }
        string admin = DefaultRoles.Admin + "_" + organization.Name;
        await _userManager.AddToRoleAsync(user, admin);

        _logger.LogInformation("Default roles created successfully");
    }

    private async Task AddClaim(Role role, List<string> operations)
    {
        var allClaims = await _roleManager.GetClaimsAsync(role);
        foreach (var operation in operations)
        {
            if (!allClaims.Any(c => c.Type == "Permission" && c.Value == operation))
            {
                await _roleManager.AddClaimAsync(role, new Claim("Permission", operation));
            }
        }
    }
}
