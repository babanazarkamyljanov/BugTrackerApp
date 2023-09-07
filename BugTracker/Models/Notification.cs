namespace BugTracker.Models;

public class Notification
{
    [Key]
    public int Id { get; set; }

    public bool IsRead { get; set; } = false;

    [ForeignKey("AssignedUser")]
    public string AssignedUserID { get; set; } = string.Empty;
    public User AssignedUser { get; set; } = null!;

    public string Controller { get; set; } = string.Empty;

    public string DetailsID { get; set; } = string.Empty;
}
