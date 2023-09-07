namespace BugTracker.Models;

public class Organization
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public ICollection<User> OrganizationUsers { get; set; } = null!;

    public ICollection<Project> Projects { get; set; } = null!;

    public Organization()
    {
        Id = Guid.NewGuid();
    }
}
