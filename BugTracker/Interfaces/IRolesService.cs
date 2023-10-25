namespace BugTracker.Interfaces;

public interface IRolesService
{
    Task CreateDefaultRoles(User user, Organization organization);
}
