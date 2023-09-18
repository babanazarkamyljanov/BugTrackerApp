namespace BugTracker.Interfaces;

public interface IRolesService
{
    Task CreateDefaultRoles(User user, Guid orgId);
}
