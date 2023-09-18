using BugTracker.Authorization;

namespace BugTracker.Controllers;

public class PermissionsController : Controller
{
    private readonly RoleManager<Role> roleManager;
    private readonly IAuthorizationService authorizationService;

    public PermissionsController(
        RoleManager<Role> roleManager,
        IAuthorizationService authorizationService)
    {
        this.roleManager = roleManager;
        this.authorizationService = authorizationService;

    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Index(string roleId)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.PermissionManageOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var model = new PermissionViewModel();
        var allPermissions = PermissionHelper();

        var role = await roleManager.FindByIdAsync(roleId);
        model.RoleId = roleId;
        model.RoleName = role.Name;

        var claims = await roleManager.GetClaimsAsync(role);
        var allClaimValues = allPermissions.Select(a => a.Value).ToList();
        var roleClaimValues = claims.Select(a => a.Value).ToList();
        var authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();

        foreach (var permission in allPermissions)
        {
            if (authorizedClaims.Any(a => a == permission.Value))
            {
                permission.Selected = true;
            }
        }
        model.RoleClaims = allPermissions;

        return View(model);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Update(PermissionViewModel model)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.PermissionManageOperations.Update);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var role = await roleManager.FindByIdAsync(model.RoleId);
        var claims = await roleManager.GetClaimsAsync(role);
        foreach (var claim in claims)
        {
            await roleManager.RemoveClaimAsync(role, claim);
        }
        var selectedClaims = model.RoleClaims.Where(a => a.Selected).ToList();
        foreach (var claim in selectedClaims)
        {
            await roleManager.AddClaimAsync(role, new Claim("Permission", claim.Value));
        }

        return RedirectToAction("Index", new { roleId = model.RoleId });
    }

    #region helpers
    public List<RoleClaimsViewModel> PermissionHelper()
    {
        var allPermissions = new List<RoleClaimsViewModel>();

        var options = Permissions.ProjectOperations.GenerateProjectOperations();
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
