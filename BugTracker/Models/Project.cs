namespace BugTracker.Models;

[Index(nameof(Key), IsUnique = true)]
public class Project
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Key { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string Priority { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty;

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
