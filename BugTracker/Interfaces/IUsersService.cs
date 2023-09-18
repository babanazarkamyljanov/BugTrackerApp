namespace BugTracker.Interfaces;

public interface IUsersService
{
    string GetCurrentUserId();

    Task<User> GetCurrentUserAsync();

    Task<User> GetUserByIdAsync(string id);

    Task<List<User>> GetOrganizationUsersAsync(Guid id);

    Task<IdentityResult> UpdateAsync(User user);
}
