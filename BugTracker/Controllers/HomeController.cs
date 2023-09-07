using System.Diagnostics;

namespace BugTracker.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext context;
    private readonly SignInManager<User> signInManager;
    private readonly UserManager<User> userManager;
    public HomeController(ILogger<HomeController> logger, 
        ApplicationDbContext context,
        SignInManager<User> signInManager,
        UserManager<User> userManager)
    {
        _logger = logger;
        this.context = context;
        this.signInManager = signInManager;
        this.userManager = userManager;
    }
    [HttpGet]
    public IActionResult Index()
    {
        var ds = new DashboardViewModel
        {
            // count each status quantity 
            Bug_Open = context.Bugs.Where(b => b.Status == "Open").Count(),
            Bug_BuildInProgress = context.Bugs.Where(b => b.Status == "Build In Progress").Count(),
            Bug_CodeReview = context.Bugs.Where(b => b.Status == "Code Review").Count(),
            Bug_FunctionalTesting = context.Bugs.Where(b => b.Status == "Functional Testing").Count(),
            Bug_Fixed = context.Bugs.Where(b => b.Status == "Fixed").Count(),
            Bug_Closed = context.Bugs.Where(b => b.Status == "Closed").Count(),

            Project_Active = context.Projects.Where(p => p.Status == "Active").Count(),
            Project_InProgress = context.Projects.Where(p => p.Status == "In Progress").Count(),
            ProjectCompleted = context.Projects.Where(p => p.Status == "Completed").Count(),
            Project_NotActive = context.Projects.Where(p => p.Status == "Not Active").Count(),
            Project_Closed = context.Projects.Where(p => p.Status == "Closed").Count()
        };
        return View(ds);
    }
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        List<Notification> model = new();
        if (signInManager.IsSignedIn(User))
        {
            var userId = userManager.GetUserId(User);
            model = await context.Notifications
                .Include(n => n.AssignedUser)
                .Where(n => n.AssignedUserID == userId)
                .ToListAsync();
        }
        return Ok(model);
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}