namespace BugTracker.Models;

public class Role : IdentityRole<string>
{
    [ForeignKey("Organization")]
    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public Role()
    {
        Id = Guid.NewGuid().ToString();
    }
}
