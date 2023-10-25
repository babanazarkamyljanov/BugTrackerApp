using BugTracker.Interfaces;

namespace BugTracker.Services;

public class AccountsService : IAccountsService
{
    private readonly UserManager<User> _userManager;
    private readonly IUserStore<User> _userStore;
    private readonly IUserEmailStore<User> _emailStore;
    private readonly ApplicationDbContext _context;
    private readonly IOrganizationService _organizationService;
    private readonly IRolesService _rolesService;

    public AccountsService(UserManager<User> userManager,
        IUserStore<User> userStore,
        ApplicationDbContext context,
        IOrganizationService organizationService,
        IRolesService rolesService)
    {
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _context = context;
        _organizationService = organizationService;
        _rolesService = rolesService;
    }

    public async Task<User> CreateUser(Guid organizationId, RegisterViewModel model, CancellationToken ct)
    {
        Organization? organization = await _context.Organizations.FindAsync(organizationId);
        if (organization == null)
        {
            throw new ArgumentException("Organization by id wasn't found", nameof(organizationId));
        }

        if (string.IsNullOrWhiteSpace(model.FirstName))
        {
            throw new ArgumentException("First Name can't be empty", nameof(model.FirstName));
        }

        if (string.IsNullOrWhiteSpace(model.LastName))
        {
            throw new ArgumentException("Last Name can't be empty", nameof(model.LastName));
        }

        User user = new User
        {
            FirstName = model.FirstName.Trim(),
            LastName = model.LastName.Trim(),
            OrganizationId = organizationId,
        };
        await _userStore.SetUserNameAsync(user, model.Email, ct);
        await _emailStore.SetEmailAsync(user, model.Email, ct);
        IdentityResult result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            return user;
        }
        else
        {
            List<string> errors = result.Errors
                .Select(e => $"{e.Code}: {e.Description}")
                .ToList();
            throw new InvalidOperationException(string.Join("; ", errors));
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
    #endregion
}



