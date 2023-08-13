namespace BugTracker.ViewModels.ManageAccount;

public class DeleteAccountViewModel
{
    public string Id { get; set; }
    public bool RequirePassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;
}
