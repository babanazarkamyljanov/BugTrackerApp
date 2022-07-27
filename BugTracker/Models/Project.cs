namespace BugTracker.Models;

public class Project
{
    public int ProjectId { get; set; }

    [Required]
    [Display(Name ="Title")]
    public string Title { get; set; }

    [Required]
    [Display(Name ="Description")]
    public string Description { get; set; }

    [Required]
    [Display(Name ="Priority")]
    public string Priority { get; set; }

    [Display(Name ="Submitted date")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("ProjectManager")]
    public string ManagerId { get; set; }
    public ApplicationUser ProjectManager { get; set; }

    public ICollection<AppUserProject> AssignedUsersForProject { get; set; }
    public Project()
    {
        CreatedDate = DateTime.Now;
        AssignedUsersForProject = new List<AppUserProject>();
    }
}
