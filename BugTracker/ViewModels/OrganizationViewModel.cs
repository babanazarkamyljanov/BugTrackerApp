namespace BugTracker.ViewModels;

public class OrganizationViewModel
{
    public Organization Organization { get; set; }
    public ICollection<ApplicationUser> OrganizationUsers { get; set; }

    public OrganizationViewModel()
    {
        OrganizationUsers = new List<ApplicationUser>();
    }
}
