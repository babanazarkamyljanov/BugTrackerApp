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

    public async Task CreateDefaultRoles(User user, Guid orgId)
    {
        var defaultRoles = DefaultRoles.GenerateDefaultRolesList();
        foreach (var roleName in defaultRoles)
        {
            string uniqueName = roleName + "_" + orgId.ToString();
            if (!await _roleManager.Roles
                .Where(r => r.Name == uniqueName && r.OrganizationId == orgId)
                .AnyAsync())
            {
                try
                {
                    await _roleManager.CreateAsync(new Role() { Name = uniqueName, OrganizationId = orgId });
                }
                catch
                {
                    throw new InvalidOperationException($"Create role failed {nameof(RolesService)}.{nameof(CreateDefaultRoles)}");
                }
            }
        }

        var orgRoles = await _roleManager.Roles
            .Where(r => r.OrganizationId == orgId)
            .ToListAsync();
        foreach (var role in orgRoles)
        {
            List<string> operations = new();
            if (role.Name == DefaultRoles.Admin + "_" + orgId.ToString())
            {
                operations = AdminPermissions.Generate();
            }
            else if (role.Name == DefaultRoles.ProjectManager + "_" + orgId.ToString())
            {
                operations = ProjectManagerPermissions.Generate();
            }
            else if (role.Name == DefaultRoles.Developer + "_" + orgId.ToString())
            {
                operations = DeveloperPermissions.Generate();
            }
            else if (role.Name == DefaultRoles.Tester + "_" + orgId.ToString())
            {
                operations = TesterPermissions.Generate();
            }
            else if (role.Name == DefaultRoles.Submitter + "_" + orgId.ToString())
            {
                operations = SubmitterPermissions.Generate();
            }

            await AddClaim(role, operations);
        }
        string admin = DefaultRoles.Admin + "_" + orgId;
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
