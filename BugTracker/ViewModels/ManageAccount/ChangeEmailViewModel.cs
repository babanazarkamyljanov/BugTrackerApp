namespace BugTracker.ViewModels.ManageAccount;

public class ChangeEmailViewModel
{
    public string Id { get; set; }

    public string Email { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "New email")]
    public string NewEmail { get; set; } = default!;
}
