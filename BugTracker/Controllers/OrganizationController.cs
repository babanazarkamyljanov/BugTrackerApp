using BugTracker.Authorization;
using BugTracker.Interfaces;
using BugTracker.Models.DTOs;

namespace BugTracker.Controllers;

public class OrganizationController : Controller
{
    private readonly ILogger<RegisterViewModel> _logger;
    private readonly IAuthorizationService _authorizationService;
    private readonly UserManager<User> _userManager;
    private readonly IOrganizationService _organizationService;
    private readonly IAccountsService _accountsService;

    public OrganizationController(ILogger<RegisterViewModel> logger,
        IAuthorizationService authorizationService,
        UserManager<User> userManager,
        IOrganizationService organizationService,
        IAccountsService accountsService)
    {
        _logger = logger;
        _authorizationService = authorizationService;
        _userManager = userManager;
        _organizationService = organizationService;
        _accountsService = accountsService;
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

        try
        {
            User user = await _accountsService.CreateUser(id, model, ct);
            await _userManager.AddToRoleAsync(user, model.Role);
            return LocalRedirect("/Organization/Index");
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(OrganizationController)}.{nameof(CreateUser)}");
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"{nameof(OrganizationController)}.{nameof(CreateUser)}");
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(OrganizationController)}.{nameof(CreateUser)}");
            ModelState.AddModelError(string.Empty, "Something went wrong");
            return View(model);
        }
    }
}
