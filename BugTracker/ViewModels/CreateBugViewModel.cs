namespace BugTracker.ViewModels;

public class CreateBugViewModel
{
    public Bug Bug { get; set; } = null!;

    public List<User> AssigneeList { get; set; } = null!;

    public List<Project> Projects { get; set; } = null!;
}
