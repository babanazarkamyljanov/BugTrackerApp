namespace BugTracker.Models;

public class Bug
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Bug Title")]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Bug Description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Bug Priority")]
    public string Priority { get; set; } = string.Empty;

    [Display(Name = "Bug Status")]
    public string Status { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    [ForeignKey("Organization")]
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    public string CreatedById { get; set; } = string.Empty;
    public User CreatedBy { get; set; } = null!;

    [ForeignKey("AssignedUser")]
    public string AssignedUserId { get; set; } = string.Empty;
    public User AssignedUser { get; set; } = null!;

    public ICollection<BugHistory> History { get; set; } = null!;

    public ICollection<BugComment> Comments { get; set; } = null!;

    public ICollection<BugFile> Files { get; set; } = null!;
}
