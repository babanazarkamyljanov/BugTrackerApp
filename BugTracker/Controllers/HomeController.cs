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