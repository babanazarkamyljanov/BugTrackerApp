namespace BugTracker.ViewModels;

public class OrganizationViewModel
{
    public Organization Organization { get; set; } = null!;

    public ICollection<User> Users { get; set; } = null!;
}
