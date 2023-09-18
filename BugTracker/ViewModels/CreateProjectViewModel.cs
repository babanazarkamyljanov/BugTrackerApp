namespace BugTracker.ViewModels;

public class CreateProjectViewModel
{
    public Project Project { get; set; } = new Project();

    public List<User> Managers { get; set; } = new List<User>();
}
