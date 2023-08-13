namespace BugTracker.Models;

[Index(nameof(ProjectKey), IsUnique = true)]
public class Project
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [Display(Name ="Title")]
    public string Title { get; set; }

    [Required]
    [Display(Name = "Project Key")]
    public string ProjectKey { get; set; }

    [Required]
    [Display(Name ="Description")]
    public string Description { get; set; }

    [Required]
    [Display(Name ="Priority")]
    public string Priority { get; set; }

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; }

    [Display(Name ="Submitted date")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("ProjectManager")]
    public string ManagerId { get; set; }
    public ApplicationUser ProjectManager { get; set; }

    [ForeignKey("CreatedBy")]
    public string CreatedById { get; set; }
    public ApplicationUser CreatedBy { get; set; }

    public ICollection<Bug> ProjectBugs { get; set; }

    public Project()
    {
        Id = new Guid();
        CreatedDate = DateTime.Now;
    }
}
