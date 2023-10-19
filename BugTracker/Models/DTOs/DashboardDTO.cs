namespace BugTracker.Models.DTOs;

public class DashboardDTO
{
    public int ProjectsCount { get; set; }

    public int BugsCount { get; set; }

    public int UsersCount { get; set; }

    public int Bug_Open { get; set; }
    public int Bug_BuildInProgress { get; set; }
    public int Bug_CodeReview { get; set; }
    public int Bug_FunctionalTesting { get; set; }
    public int Bug_Fixed { get; set; }
    public int Bug_Closed { get; set; }

    public int Project_Open { get; set; }
    public int Project_InProgress { get; set; }
    public int Project_Completed { get; set; }
    public int Project_Closed { get; set; }

    public int Bug_Low { get; set; }
    public int Bug_Medium { get; set; }
    public int Bug_High { get; set; }

    public int Project_Low { get; set; }
    public int Project_Medium { get; set; }
    public int Project_High { get; set; }
}
