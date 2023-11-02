namespace BugTracker.Interfaces;

public interface IAccountsService
{
    Task<User> CreateUser(Guid organizationId, RegisterViewModel model, CancellationToken ct);
}
