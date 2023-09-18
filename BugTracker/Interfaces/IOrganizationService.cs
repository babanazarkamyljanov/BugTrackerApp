namespace BugTracker.Interfaces;

public interface IOrganizationService
{
    Task<Organization> Create(string name, CancellationToken cancellationToken);

    Task Delete(Guid id, CancellationToken cancellationToken);
}
