namespace BugTracker.Models;

public class AppUserProject
{
    public string AppUserId { get; set; }
    public ApplicationUser AppUser { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; }
}
