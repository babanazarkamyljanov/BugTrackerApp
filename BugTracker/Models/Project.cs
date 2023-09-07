namespace BugTracker.Models;

[Index(nameof(Key), IsUnique = true)]
public class Project
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Project Key")]
    public string Key { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Priority")]
    public string Priority { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Status")]
    public string Status { get; set; } = string.Empty;

    [Display(Name = "Created date")]
    public DateTime CreatedDate { get; set; }

    [ForeignKey("Organization")]
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    [ForeignKey("Manager")]
    public string ManagerId { get; set; }
    public User Manager { get; set; }

    [ForeignKey("CreatedBy")]
    public string CreatedById { get; set; }
    public User CreatedBy { get; set; }

    public ICollection<Bug> Bugs { get; set; }

    public Project()
    {
        Id = new Guid();
        CreatedDate = DateTime.Now;
    }
}
