using BugTracker.Interfaces;

namespace BugTracker.Services;

public class UsersService : IUsersService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<User> _userManager;

    public UsersService(IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public string GetCurrentUserId()
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new ArgumentException($"HttpContext is null. {nameof(UsersService)}.{nameof(GetCurrentUserId)}");
        }

        var claim = _httpContextAccessor
                                .HttpContext
                                .User
                                .FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null)
        {
            throw new ArgumentException($"Claim is null. {nameof(UsersService)}.{nameof(GetCurrentUserId)}");
        }

        return claim.Value;
    }

    public async Task<User> GetCurrentUserAsync()
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new ArgumentException("HttpContext is null", nameof(_httpContextAccessor.HttpContext));
        }

        var claim = _httpContextAccessor
                                .HttpContext
                                .User
                                .FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null)
        {
            throw new ArgumentException("Claim is null", nameof(claim));
        }

        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == claim.Value);
        if (user == null)
        {
            throw new ArgumentException($"Current user wasn't found. {nameof(UsersService)}.{nameof(GetCurrentUserAsync)}");
        }

        return user;
    }

    public async Task<User> GetUserByIdAsync(string id)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            throw new ArgumentException($"user wasn't found {nameof(UsersService)}.{nameof(GetCurrentUserId)}");
        }

        return user;
    }

    public async Task<List<User>> GetOrganizationUsersAsync(Guid id)
    {
        var users = await _userManager.Users
            .Where(u => u.OrganizationId == id)
            .ToListAsync();

        if (users == null)
        {
            throw new ArgumentException($"users weren't found {nameof(UsersService)}.{nameof(GetOrganizationUsersAsync)}");
        }
        return users;
    }
}
