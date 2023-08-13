namespace BugTracker.Services;

public class NotificationService
{
    private readonly ApplicationDbContext context;
    private readonly SignInManager<ApplicationUser> signInManager;
    private readonly UserManager<ApplicationUser> userManager;
    private IHttpContextAccessor httpContextAccessor;
    private readonly IHubContext<CommonHub> hubContext;

    public NotificationService(ApplicationDbContext context,
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        IHubContext<CommonHub> hubContext)
    {
        this.context = context;
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.httpContextAccessor = httpContextAccessor;
        this.hubContext = hubContext;
    }

    public List<Notification> GetNotifications()
    {
        List<Notification> notifications = new();
        var userId = userManager.GetUserId(httpContextAccessor.HttpContext?.User);
        notifications = context.Notifications.Where(n => n.AssignedUserID == userId).ToList();
        return notifications;
    }

    public async Task<string> ResetNotificationCount()
    {
        var userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        var user = await userManager.FindByIdAsync(userId);
        user.NotificationCount = 0;
        await userManager.UpdateAsync(user);
        await hubContext.Clients.User(userId).SendAsync("GetNotifications", user.NotificationCount);
        return "success";
    }
}
