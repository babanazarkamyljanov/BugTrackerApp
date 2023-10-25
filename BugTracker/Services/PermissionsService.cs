using BugTracker.Authorization;
using BugTracker.Interfaces;

namespace BugTracker.Services;

public class PermissionsService : IPermissionsService
{
    private readonly IUsersService _usersService;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;

    public PermissionsService(IUsersService usersService,
        UserManager<User> userManager,
        RoleManager<Role> roleManager)
    {
        _usersService = usersService;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<PermissionViewModel> Get(string roleId, CancellationToken ct)
    {
        string userId = _usersService.GetCurrentUserId();
        User? currentUser = await _userManager.Users
            .Where(u => u.Id == userId)
            .FirstOrDefaultAsync();
        if (currentUser == null)
        {
            throw new InvalidOperationException("Current logged in user wasn't found");
        }

        Role? role = await _roleManager.Roles
            .Where(r => r.Id == roleId && r.OrganizationId == currentUser.OrganizationId)
            .FirstOrDefaultAsync(ct);
        if (role == null)
        {
            throw new ArgumentException("Role by id wasn't found", nameof(roleId));
        }

        PermissionViewModel model = new PermissionViewModel();
        List<RoleClaimsViewModel> allPermissions = PermissionHelper();

        model.RoleId = roleId;
        model.RoleName = role.Name.Replace(("_" + currentUser.OrganizationId), "");

        IList<Claim> claims = await _roleManager.GetClaimsAsync(role);
        List<string> allClaimValues = allPermissions.Select(a => a.Value).ToList();
        List<string> roleClaimValues = claims.Select(a => a.Value).ToList();
        List<string> authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();

        foreach (var permission in allPermissions)
        {
            if (authorizedClaims.Any(a => a == permission.Value))
            {
                permission.Selected = true;
            }
        }
        model.RoleClaims = allPermissions;

        return model;
    }

    public async Task Update(PermissionViewModel model)
    {
        Role role = await _roleManager.FindByIdAsync(model.RoleId);
        if (role == null)
        {
            throw new ArgumentException("Role by id wasn't found", nameof(model.RoleId));
        }
        IList<Claim> claims = await _roleManager.GetClaimsAsync(role);

        foreach (var claim in claims)
        {
            await _roleManager.RemoveClaimAsync(role, claim);
        }
        List<RoleClaimsViewModel> selectedClaims = model.RoleClaims.Where(a => a.Selected).ToList();
        foreach (var claim in selectedClaims)
        {
            await _roleManager.AddClaimAsync(role, new Claim("Permission", claim.Value));
        }
    }

    #region helpers
    private List<RoleClaimsViewModel> PermissionHelper()
    {
        List<RoleClaimsViewModel> allPermissions = new List<RoleClaimsViewModel>();

        List<string> options = Permissions.ProjectOperations.GenerateProjectOperations();
        foreach (var permission in options)
        {
            allPermissions.Add(new RoleClaimsViewModel
            {
                Value = permission,
                Type = "Permission"
            });
        }

        options = Permissions.BugOperations.GenerateBugOperations();
        foreach (var permission in options)
        {
            allPermissions.Add(new RoleClaimsViewModel
            {
                Value = permission,
                Type = "Permission"
            });
        }

        options = Permissions.AccountManageOperations.GenerateAccountManageOperations();
        foreach (var permission in options)
        {
            allPermissions.Add(new RoleClaimsViewModel
            {
                Value = permission,
                Type = "Permission"
            });
        }

        options = Permissions.RoleManageOperations.GenerateRoleManageOperations();
        foreach (var permission in options)
        {
            allPermissions.Add(new RoleClaimsViewModel
            {
                Value = permission,
                Type = "Permission"
            });
        }

        options = Permissions.PermissionManageOperations.GeneratePermissionManageOperations();
        foreach (var permission in options)
        {
            allPermissions.Add(new RoleClaimsViewModel
            {
                Value = permission,
                Type = "Permission"
            });
        }

        return allPermissions;
    }
    #endregion
}
