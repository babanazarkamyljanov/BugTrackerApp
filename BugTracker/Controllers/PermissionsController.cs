using BugTracker.Authorization;
using BugTracker.Interfaces;

namespace BugTracker.Controllers;

public class PermissionsController : Controller
{
    private readonly ILogger<PermissionsController> _logger;
    private readonly IAuthorizationService _authorizationService;
    private readonly IPermissionsService _permissionsService;

    public PermissionsController(ILogger<PermissionsController> logger,
        IAuthorizationService authorizationService,
        IPermissionsService permissionsService)
    {
        _logger = logger;
        _authorizationService = authorizationService;
        _permissionsService = permissionsService;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<PermissionViewModel>> Index(string roleId, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await _authorizationService
            .AuthorizeAsync(User, Permissions.PermissionManageOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }
        try
        {
            PermissionViewModel model = await _permissionsService.Get(roleId, ct);
            return View(model);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"{nameof(PermissionsController)}.{nameof(Index)}");
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(PermissionsController)}.{nameof(Index)}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(PermissionsController)}.{nameof(Index)}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Update(PermissionViewModel model)
    {
        AuthorizationResult isAuthorized = await _authorizationService
            .AuthorizeAsync(User, Permissions.PermissionManageOperations.Update);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            await _permissionsService.Update(model);
            return RedirectToAction(nameof(Index), new { roleId = model.RoleId });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(PermissionsController)}.{nameof(Update)}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(PermissionsController)}.{nameof(Update)}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }
}
