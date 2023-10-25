namespace BugTracker.Interfaces;

public interface IPermissionsService
{
    Task<PermissionViewModel> Get(string roleId, CancellationToken ct);

    Task Update(PermissionViewModel model);
}
