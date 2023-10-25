using BugTracker.Models.DTOs;

namespace BugTracker.Interfaces;

public interface IOrganizationService
{
    Task<Organization> Create(string name, CancellationToken cancellationToken);

    Task Edit(EditOrganizationDTO organizationDTO, CancellationToken ct);

    Task<GetOrganizationDTO> GetOrganization(CancellationToken ct);

    Task Delete(Guid id, CancellationToken cancellationToken);

    Task<bool> IsAlreadyExists(string name, CancellationToken ct);
}
