namespace BugTracker.ViewModels;

public class CreateBugViewModel
{
    public Bug Bug { get; set; }
    public List<AppUserViewModel> Users { get; set; }
    public List<Project> Projects { get; set; }

    public CreateBugViewModel()
    {
        Users = new List<AppUserViewModel>();
        Projects = new List<Project>();
    }
}
