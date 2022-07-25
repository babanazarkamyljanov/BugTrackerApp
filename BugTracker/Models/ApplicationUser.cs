namespace BugTracker.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(30, ErrorMessage = "The first name should have a maximum of 30 characters")]
    public string FirstName { get; set; }

    [Required]
    [StringLength(30, ErrorMessage = "The last name should have a maximum of 30 characters")]
    public string LastName { get; set; }

    [NotMapped]
    public string RoleName { get; set; }
    public ICollection<AppUserBug> AppUsersForBugs { get; set; }
    public ICollection<AppUserProject> AppUsersForProjects { get; set; }

    public ApplicationUser()
    {
        AppUsersForBugs = new List<AppUserBug>();
        AppUsersForProjects = new List<AppUserProject>();
    }
}
