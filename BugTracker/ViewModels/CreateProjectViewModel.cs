namespace BugTracker.ViewModels;

public class CreateProjectViewModel
{
    public Project Project { get; set; } = null!;

    public List<User> Assignee { get; set; } = null!;

    public List<User> Managers { get; set; } = null!;
}
