using BugTracker.Authorization;
using BugTracker.Interfaces;

namespace BugTracker.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IUserStore<User> _userStore;
    private readonly IUserEmailStore<User> _emailStore;
    private readonly ILogger<RegisterViewModel> _logger;
    private readonly IOrganizationService _organizationService;

    public AccountController(
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IUserStore<User> userStore,
        SignInManager<User> signInManager,
        ILogger<RegisterViewModel> logger,
        IOrganizationService organizationService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _organizationService = organizationService;
    }

    // GET: Account/Login
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        //Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return RedirectToAction(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return View("Lockout");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        return View(model);
    }

    // GET: Account/Register
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // POST: Accoount/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model,
        string? returnUrl = null,
        CancellationToken ct = default)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(model.FirstName))
            {
                ModelState.AddModelError(nameof(model.FirstName), "First Name can't be empty");
            }

            if (string.IsNullOrWhiteSpace(model.LastName))
            {
                ModelState.AddModelError(nameof(model.LastName), "Last Name can't be empty");
            }

            Organization organization = new Organization()
            {
                Id = Guid.Empty
            };

            try
            {
                organization = await _organizationService.Create(model.OrganizationName, ct);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"{nameof(AccountController)}.{nameof(Register)}");
                ModelState.AddModelError(nameof(model.OrganizationName), ex.Message);
                return View(model);
            }

            User user = new User()
            {
                FirstName = model.FirstName.Trim(),
                LastName = model.LastName.Trim(),
                OrganizationId = organization.Id,
            };

            await _userStore.SetUserNameAsync(user, model.Email, ct);
            await _emailStore.SetEmailAsync(user, model.Email, ct);
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                try
                {
                    await CreateDefaultRoles(user, organization);
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError(nameof(model.OrganizationName), ex.Message);
                }
                
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToLocal(returnUrl);
            }
            AddErrors(result);
        }
        return View(model);
    }

    // POST: Account/Logout
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation(4, "User logged out.");
        return RedirectToAction("Index", "Home");
    }

    public IActionResult AccessDenied()
    {
        return View();
    }

    #region Helpers

    private IUserEmailStore<User> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<User>)_userStore;
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction(nameof(HomeController.Index), "Home");
        }
    }

    private async Task CreateDefaultRoles(User user, Organization organization)
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
                    throw new ArgumentException("Creating default roles was failed for organization", nameof(organization));
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

    #endregion
}
