using BugTracker.Authorization;
using System.Data;

namespace BugTracker.Controllers;

public class OrganizationController : Controller
{
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IUserStore<ApplicationUser> userStore;
    private readonly IUserEmailStore<ApplicationUser> emailStore;
    private readonly ILogger<RegisterViewModel> logger;
    private readonly ApplicationDbContext context;
    private readonly IAuthorizationService authorizationService;

    public OrganizationController(
        ApplicationDbContext context, 
        IHttpContextAccessor httpContextAccessor, 
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        ILogger<RegisterViewModel> logger,
        IAuthorizationService authorizationService)
    {
        this.context = context;
        this.httpContextAccessor = httpContextAccessor;
        this.userManager = userManager;
        this.userStore = userStore;
        this.emailStore = GetEmailStore();
        this.logger = logger;
        this.authorizationService = authorizationService;
    }

    // GET
    // Organization
    public async Task<IActionResult> Index()
    {
        // get current user
        var currentUserId = httpContextAccessor
                            .HttpContext
                            .User
                            .FindFirst(ClaimTypes.NameIdentifier)
                            .Value;
        var currentUser = await userManager.FindByIdAsync(currentUserId);

        // find organization where current user is in this organization
        var model = await context.Organizations
            .SingleOrDefaultAsync(o => o.OrganizationId == currentUser.OrganizationId);

        // find all organization users
        var orgUsers = await userManager.Users
            .Where(u => u.OrganizationId == model.OrganizationId && u != currentUser)
            .ToListAsync();

        // fill user roles property
        foreach (var user in orgUsers)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            string roles = "";
            for (int i = 0; i < userRoles.Count - 1; i++)
            {
                roles += userRoles[i] + ' ' + '/' + ' ';
            }
            roles += userRoles[userRoles.Count - 1];
            user.UserRoles = roles;
        }

        // create view model
        var organizationVM = new OrganizationViewModel
        {
            Organization = model,
            OrganizationUsers = orgUsers
        };

        return View(organizationVM);
    }

    // POST
    // Change the name of organization
    [HttpPost]
    public async Task<ActionResult> ChangeName(Guid id , string newName)
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
        if(model == null)
        {
            return Json("Organization not found");
        }

        try
        {
            
            model.OrganizationName = newName;
            context.Update(model);
            await context.SaveChangesAsync();
            return Json("change name succes");
        }
        catch (Exception e)
        {
            return Json(e.Message);
        }
    }

    // GET
    // Create a user for organization
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

    // POST
    // Create a user for organization
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
            var user = new ApplicationUser
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
                // add user to given roles
                foreach (var role in model.Roles)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
                logger.LogInformation("User created a new account with password.");
                return LocalRedirect("/Organization/Index");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
        if (!userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<ApplicationUser>)userStore;
    }
}

