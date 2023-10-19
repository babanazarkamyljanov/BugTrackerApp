using BugTracker.Authorization;
using BugTracker.Interfaces;
using BugTracker.Models.DTOs;

namespace BugTracker.Controllers;

public class OrganizationController : Controller
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly UserManager<User> userManager;
    private readonly IUserStore<User> userStore;
    private readonly IUserEmailStore<User> emailStore;
    private readonly ILogger<RegisterViewModel> _logger;
    private readonly ApplicationDbContext context;
    private readonly IAuthorizationService authorizationService;
    private readonly IOrganizationService _organizationService;

    public OrganizationController(
        ApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor,
        UserManager<User> userManager,
        IUserStore<User> userStore,
        ILogger<RegisterViewModel> logger,
        IAuthorizationService authorizationService,
        IOrganizationService organizationService)
    {
        this.context = context;
        this.httpContextAccessor = httpContextAccessor;
        this.userManager = userManager;
        this.userStore = userStore;
        this.emailStore = GetEmailStore();
        _logger = logger;
        this.authorizationService = authorizationService;
        _organizationService = organizationService;
    }

    public async Task<ActionResult<GetOrganizationDTO>> Index(CancellationToken ct = default)
    {
        try
        {
            var result = await _organizationService.GetOrganization(ct);
            return result;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(OrganizationController)}.{nameof(Index)}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(OrganizationController)}.{nameof(Index)}");
            ViewData["ErrorMessage"] = "Something went wrong";
            return View();
        }
    }

    [HttpPost]
    public async Task<ActionResult> ChangeName(Guid id, string newName)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.OrganizationManageOperations.Edit);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (newName == null)
        {
            return Json("ValidationInvalid");
        }

        var model = await context.Organizations.FindAsync(id);
        if (model == null)
        {
            return Json("Organization not found");
        }

        try
        {

            model.Name = newName;
            context.Update(model);
            await context.SaveChangesAsync();
            return Json("change name succes");
        }
        catch (Exception e)
        {
            return Json(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> CreateUser(Guid id)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.OrganizationManageOperations.CreateUser);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (await context.Organizations.FindAsync(id) is null)
        {
            return NotFound();
        }

        RegisterViewModel model = new();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(Guid id, RegisterViewModel model)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.OrganizationManageOperations.CreateUser);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            var user = new User
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                OrganizationId = id
            };

            await userStore.SetUserNameAsync(user, model.Email, CancellationToken.None);
            await emailStore.SetEmailAsync(user, model.Email, CancellationToken.None);
            var result = await userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                foreach (var role in model.Roles)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                _logger.LogInformation("User created a new account with password.");
                return LocalRedirect("/Organization/Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    private IUserEmailStore<User> GetEmailStore()
    {
        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<User>)userStore;
    }
}

