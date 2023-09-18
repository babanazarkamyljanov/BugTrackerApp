namespace BugTracker.ViewModels;

public class PermissionViewModel
{
    public string RoleId { get; set; } = string.Empty;

    public string RoleName { get; set; } = string.Empty;

    public IList<RoleClaimsViewModel> RoleClaims { get; set; } = null!;
}

public class RoleClaimsViewModel
{
    public string Type { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public bool Selected { get; set; }
}
