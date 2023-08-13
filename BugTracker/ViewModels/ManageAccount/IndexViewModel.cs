namespace BugTracker.ViewModels.ManageAccount;

public class IndexViewModel
{
    public string Id { get; set; }

    public string Username { get; set; }

    [Required]
    [Phone]
    [Display(Name = "Phone number")]
    public string PhoneNumber { get; set; }
}