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

        var user = await _userManager.Users
            .Where(u => u.Id == claim.Value)
            .FirstOrDefaultAsync();
        if (user == null)
        {
            throw new ArgumentException("Current logged user wasn't found in the db");
        }

        return user;
    }

    public async Task<User> GetUserByIdAsync(string id)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id.ToString() == id);

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
            throw new ArgumentException("Organization users weren't found");
        }
        return users;
    }

    public async Task<IdentityResult> UpdateAsync(User user)
    {
        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return result;
        }
        else
        {
            throw new InvalidOperationException($"Update user failed. {nameof(UsersService)}.{nameof(UpdateAsync)}");
        }
    }
}
