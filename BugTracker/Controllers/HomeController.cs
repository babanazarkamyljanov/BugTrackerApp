using System.Diagnostics;

namespace BugTracker.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext context;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly UserManager<ApplicationUser> userManager;
    public HomeController(ILogger<HomeController> logger, 
        ApplicationDbContext context,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        this.context = context;
        this.signInManager = signInManager;
        this.userManager = userManager;
    }

    public IActionResult Index()
    {
        var ds = new DashboardViewModel();

        // count each status quantity 
        ds.Bug_Open = context.Bugs.Where(b => b.Status == "Open").Count();
        ds.Bug_BuildInProgress = context.Bugs.Where(b => b.Status == "Build In Progress").Count();
        ds.Bug_CodeReview = context.Bugs.Where(b => b.Status == "Code Review").Count();
        ds.Bug_FunctionalTesting = context.Bugs.Where(b => b.Status == "Functional Testing").Count();
        ds.Bug_Fixed = context.Bugs.Where(b => b.Status == "Fixed").Count();
        ds.Bug_Closed = context.Bugs.Where(b => b.Status == "Closed").Count();
   
        ds.Project_Active = context.Projects.Where(p => p.Status == "Active").Count();
        ds.Project_InProgress = context.Projects.Where(p => p.Status == "In Progress").Count();
        ds.ProjectCompleted = context.Projects.Where(p => p.Status == "Completed").Count();
        ds.Project_NotActive = context.Projects.Where(p => p.Status == "Not Active").Count();
        ds.Project_Closed = context.Projects.Where(p => p.Status == "Closed").Count();
        return View();
    }
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        List<Notification> model = new();
        if (signInManager.IsSignedIn(User))
        {
            var userId = userManager.GetUserId(User);
            model = await context.Notifications.Where(n => n.AssignedUserID == userId).ToListAsync();
        }
        return Ok(model);
    }
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}