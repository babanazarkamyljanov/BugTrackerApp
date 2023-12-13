using BugTracker.Authorization;
using BugTracker.Interfaces;
using BugTracker.Models.DTOs;

namespace BugTracker.Controllers;

public class OrganizationController : Controller
{
    private readonly ILogger<OrganizationController> _logger;
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;
    private readonly IUserEmailStore<User> _emailStore;
    private readonly IAuthorizationService _authorizationService;
    private readonly IOrganizationService _organizationService;

    public OrganizationController(ILogger<OrganizationController> logger,
        UserManager<User> userManager,
        IUserStore<User> userStore,
        IAuthorizationService authorizationService,
        IOrganizationService organizationService)
    {
        _logger = logger;
        _authorizationService = authorizationService;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _organizationService = organizationService;
    }

    [HttpGet]
    public async Task<ActionResult<GetOrganizationDTO>> Index(CancellationToken ct = default)
    {
        try
        {
            GetOrganizationDTO result = await _organizationService.GetOrganization(ct);
            return View(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(OrganizationController)}.{nameof(Index)}");
            return StatusCode(StatusCodes.Status401Unauthorized, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(OrganizationController)}.{nameof(Index)}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Edit(EditOrganizationDTO dto, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await _authorizationService
            .AuthorizeAsync(User, Permissions.OrganizationManageOperations.Edit);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            await _organizationService.Edit(dto, ct);
            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(OrganizationController)}.{nameof(Edit)}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    [HttpGet]
    public async Task<IActionResult> CreateUser(Guid id)
    {
        AuthorizationResult isAuthorized = await _authorizationService
            .AuthorizeAsync(User, Permissions.OrganizationManageOperations.CreateUser);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        RegisterViewModel model = new();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(Guid id, RegisterViewModel model, CancellationToken ct)
    {
        AuthorizationResult isAuthorized = await _authorizationService
            .AuthorizeAsync(User, Permissions.OrganizationManageOperations.CreateUser);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        User user = new User()
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            OrganizationId = id,
        };

        await _userStore.SetUserNameAsync(user, model.Email, ct);
        await _emailStore.SetEmailAsync(user, model.Email, ct);
        IdentityResult result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, model.Role);
            return RedirectToAction("Index", "Organization");
        }
        else
        {
            AddErrors(result);
            return View(model);
        }
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

    #endregion
}
