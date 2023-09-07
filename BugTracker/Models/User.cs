namespace BugTracker.Models;

public class User : IdentityUser
{
    [Required]
    [StringLength(30, ErrorMessage = "The first name should have a maximum of 30 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(30, ErrorMessage = "The last name should have a maximum of 30 characters")]
    public string LastName { get; set; } = string.Empty;

    [ForeignKey("Organization")]
    public Guid OrganizationId { get; set; }

    public Organization Organization { get; set; } = null!;

    public int NotificationCount { get; set; }

    public byte[] AvatarPhoto { get; set; } = null!;

    [NotMapped]
    public string UserRoles { get; set; } = string.Empty;

    [NotMapped]
    public string RoleName { get; set; } = string.Empty;
}
