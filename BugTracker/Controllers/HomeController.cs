using BugTracker.Interfaces;
using BugTracker.Models.DTOs;

namespace BugTracker.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IDashboardService _dashboardService;

    public HomeController(ILogger<HomeController> logger,
        IDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<ActionResult<DashboardDTO>> Index(CancellationToken ct = default)
    {
        try
        {
            DashboardDTO result = await _dashboardService.Get(ct);
            return View(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"{nameof(HomeController)}.{nameof(Index)}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(HomeController)} . {nameof(Index)}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    //[HttpGet]
    //public async Task<IActionResult> GetNotifications()
    //{
    //    List<Notification> model = new();
    //    if (signInManager.IsSignedIn(User))
    //    {
    //        var userId = userManager.GetUserId(User);
    //        model = await context.Notifications
    //            .Include(n => n.AssignedUser)
    //            .Where(n => n.AssignedUserID == userId)
    //            .ToListAsync();
    //    }
    //    return Ok(model);
    //}

    //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    //public IActionResult Error()
    //{
    //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    //}
}