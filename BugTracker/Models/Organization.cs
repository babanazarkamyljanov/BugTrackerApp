namespace BugTracker.Models;

public class Organization
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public string Name { get; set; } = string.Empty;

    public ICollection<User> OrganizationUsers { get; set; } = null!;

    public ICollection<Project> Projects { get; set; } = null!;
}
