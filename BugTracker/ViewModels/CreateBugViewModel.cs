namespace BugTracker.ViewModels;

public class CreateBugViewModel
{
    public Bug Bug { get; set; }
    public List<User> AssigneeList { get; set; }
    public List<Project> Projects { get; set; }

    public CreateBugViewModel()
    {
        AssigneeList = new List<User>();
        Projects = new List<Project>();
    }
}
