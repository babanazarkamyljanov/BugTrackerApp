namespace BugTracker.Models;

[Index(nameof(Key), IsUnique = true)]
public class Project
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

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
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [ForeignKey("Organization")]
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    [ForeignKey("Manager")]
    public string ManagerId { get; set; } = string.Empty;
    public User Manager { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    public string CreatedById { get; set; } = string.Empty;
    public User CreatedBy { get; set; } = null!;

    public ICollection<Bug> Bugs { get; set; } = null!;
}
