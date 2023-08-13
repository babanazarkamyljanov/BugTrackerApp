namespace BugTracker.Models;

public class Organization
{
    [Key]
    public Guid OrganizationId { get; set; }

    [Required]
    public string OrganizationName { get; set; }
    public string CreatedById { get; set; }
    //public ICollection<ApplicationUser> OrganizationUsers { get; set; }
    //public ICollection<Project> Projects { get; set; }

    public Organization()
    {
        //Projects = new List<Project>();
        //OrganizationUsers = new List<ApplicationUser>();
        OrganizationId = Guid.NewGuid();
    }
}
