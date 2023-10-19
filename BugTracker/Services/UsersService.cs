using BugTracker.Interfaces;

namespace BugTracker.Services;

public class UsersService : IUsersService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UsersService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentUserId()
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            throw new ArgumentException("HttpContext is null");
        }

        Claim? claim = _httpContextAccessor
                                .HttpContext
                                .User
                                .FindFirst(ClaimTypes.NameIdentifier);
        if (claim == null)
        {
            throw new ArgumentException($"Claim is null. {nameof(UsersService)}.{nameof(GetCurrentUserId)}");
        }

        return claim.Value;
    }
}
