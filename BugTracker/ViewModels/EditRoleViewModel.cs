namespace BugTracker.ViewModels;

public class EditRoleViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role Name id required")]
    public string RoleName { get; set; } = string.Empty;

    public List<string> Users { get; set; } = null!;
}
