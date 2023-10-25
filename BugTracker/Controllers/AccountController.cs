using BugTracker.Interfaces;

namespace BugTracker.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;
    private readonly IUserEmailStore<User> _emailStore;
    private readonly ILogger<RegisterViewModel> _logger;
    private readonly IOrganizationService _organizationService;
    private readonly IRolesService _rolesService;
    private readonly IAccountsService _accountsService;

    public AccountController(
        UserManager<User> userManager,
        IUserStore<User> userStore,
        SignInManager<User> signInManager,
        ILogger<RegisterViewModel> logger,
        IOrganizationService organizationService,
        IRolesService rolesService,
        IAccountsService accountsService)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _organizationService = organizationService;
        _rolesService = rolesService;
        _accountsService = accountsService;
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
    public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return RedirectToAction("Index", "Home");
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

        // If we got this far, something failed, redisplay form
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
            Organization organization = new Organization()
            {
                Id = Guid.Empty
            };

            User user = new User()
            {
                Id = ""
            };

            try
            {
                organization = await _organizationService.Create(model.OrganizationName, ct);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"{nameof(AccountController)}.{nameof(Register)}");
                ModelState.AddModelError("OrganizationName", ex.Message);
                return View(model);
            }

            try
            {
                user = await _accountsService.CreateUser(organization.Id, model, ct);

                try
                {
                    await _rolesService.CreateDefaultRoles(user, organization);
                }
                catch (ArgumentException ex)
                {
                    _logger.LogError(ex, $"{nameof(AccountController)}.{nameof(Register)}");
                    ModelState.AddModelError(string.Empty, ex.Message);
                    return View(model);
                }

                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return RedirectToAction("RegisterConfirmation", new { email = model.Email, returnUrl = returnUrl });
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect("/");
                }
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"{nameof(AccountController)}.{nameof(Register)}");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, $"{nameof(AccountController)}.{nameof(Register)}");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(AccountController)}.{nameof(Register)}");
                ModelState.AddModelError(string.Empty, "Something went wrong");
                return View(model);
            }
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

    private User CreateUser()
    {
        try
        {
            return Activator.CreateInstance<User>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(Models.User)}'. " +
                $"Ensure that '{nameof(Models.User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    private IUserEmailStore<User> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<User>)_userStore;
    }
}
