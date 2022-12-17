namespace BugTracker.ViewModels;

public class DashboardViewModel
{
    // Bug statuses
    public int Bug_Open { get; set; }
    public int Bug_BuildInProgress { get; set; }
    public int Bug_CodeReview { get; set; }
    public int Bug_FunctionalTesting { get; set; }
    public int Bug_Fixed { get; set; }
    public int Bug_Closed { get; set; }

    // Project statuses
    public int Project_Active { get; set; }
    public int Project_InProgress { get; set; }
    public int ProjectCompleted { get; set; }
    public int Project_NotActive { get; set; }
    public int Project_Closed { get; set; }
}
