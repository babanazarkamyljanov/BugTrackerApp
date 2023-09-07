namespace BugTracker.ViewModels;

public class CreateRoleViewModel
{
    [Required]
    public string RoleName { get; set; } = string.Empty;
}
