namespace BugTracker.Models;

public class Notification
{
    public int ID { get; set; }
    public bool IsRead { get; set; }

    [ForeignKey("AssignedUser")]
    public string AssignedUserID { get; set; }
    public ApplicationUser AssignedUser { get; set; }

    public string Controller { get; set; }
    public int DetailsID { get; set; }
    public Notification()
    {
        IsRead = false;
    }
}
