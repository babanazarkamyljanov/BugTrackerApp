namespace BugTracker.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(30, ErrorMessage = "The first name should have a maximum of 30 characters")]
    public string FirstName { get; set; }

    [Required]
    [StringLength(30, ErrorMessage = "The last name should have a maximum of 30 characters")]
    public string LastName { get; set; }

    [ForeignKey("Organization")]
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; }

    public int NotificationCount { get; set; }

    public byte[] AvatarPhoto { get; set; }

    [NotMapped]
    public string UserRoles { get; set; }
    [NotMapped]
    public string RoleName { get; set; }
}
