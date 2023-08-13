namespace BugTracker.ViewModels;

public class CreateBugViewModel
{
    public Bug Bug { get; set; }
    public List<ApplicationUser> AssigneeList { get; set; }
    public List<Project> Projects { get; set; }

    public CreateBugViewModel()
    {
        AssigneeList = new List<ApplicationUser>();
        Projects = new List<Project>();
    }
}
