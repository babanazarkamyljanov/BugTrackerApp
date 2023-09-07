using BugTracker.Authorization;
using Microsoft.AspNetCore.Http;

namespace BugTracker.Controllers;

public class RolesController : Controller
{
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly UserManager<User> userManager;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly IAuthorizationService authorizationService;

    public RolesController(
        RoleManager<IdentityRole> roleManager,
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService
        )
    {
        this.roleManager = roleManager;
        this.userManager = userManager;
        this.httpContextAccessor = httpContextAccessor;
        this.authorizationService = authorizationService;
    }

    public async Task<IActionResult> Index()
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.RoleManageOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var roles = await roleManager.Roles.ToListAsync();
        return View(roles);
    }

    [HttpPost]
    public async Task<IActionResult> AddRole(string roleName)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.RoleManageOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (roleName != null)
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> ManageUsersInRole(string roleId)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.AccountManageOperations.View);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        ViewBag.roleId = roleId;
        var role = await roleManager.FindByIdAsync(roleId);
        ViewBag.roleName = role.Name;
        if (role == null)
        {
            ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
            return View("NotFound");
        }

        var model = new List<UserRoleViewModel>();
        var currentUser = await GetCurrentUser();
        var organizationUsers = await userManager.Users.Where(u => u.OrganizationId == currentUser.OrganizationId).ToListAsync();
        foreach (var user in organizationUsers)
        {
            var userRoleViewModel = new UserRoleViewModel
            {
                UserId = user.Id,
                UserName = user.UserName
            };
            if (await userManager.IsInRoleAsync(user, role.Name))
            {
                userRoleViewModel.IsSelected = true;
            }
            else
            {
                userRoleViewModel.IsSelected = false;
            }
            model.Add(userRoleViewModel);
        }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ManageUsersInRole(List<UserRoleViewModel> model, string roleId)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.AccountManageOperations.View);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var role = await roleManager.FindByIdAsync(roleId);
        if (role == null)
        {
            ViewBag.ErrorMessage = $"Role with Id = {roleId} cannot be found";
            return View("NotFound");
        }
        for (int i = 0; i < model.Count; i++)
        {
            var user = await userManager.FindByIdAsync(model[i].UserId);
            IdentityResult result = null;

            if (model[i].IsSelected && !(await userManager.IsInRoleAsync(user, role.Name)))
            {
                result = await userManager.AddToRoleAsync(user, role.Name);
            }
            else if (!model[i].IsSelected && await userManager.IsInRoleAsync(user, role.Name))
            {
                result = await userManager.RemoveFromRoleAsync(user, role.Name);
            }
            else
            {
                continue;
            }
            if (result.Succeeded)
            {
                if (i < model.Count - 1)
                    continue;
                else
                    return RedirectToAction("EditRole", new { Id = roleId });
            }
        }
        return RedirectToAction("ManageUsersInRole", new { Id = roleId });
    }

    // get current user
    private async Task<User> GetCurrentUser()
    {
        var id = httpContextAccessor
                                .HttpContext
                                .User
                                .FindFirst(ClaimTypes.NameIdentifier)
                                .Value;
        return await userManager.Users.SingleOrDefaultAsync(u => u.Id == id);
    }
}
