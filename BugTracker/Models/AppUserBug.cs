namespace BugTracker.Models;

public class AppUserBug
{
    public string AppUserId { get; set; }
    public ApplicationUser AppUser { get; set; }

    public int BugId { get; set; }
    public Bug Bug { get; set; }
}
