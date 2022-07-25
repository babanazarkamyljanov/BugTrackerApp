namespace BugTracker.ViewModels;

public class CreateProjectViewModel
{
    public Project Project { get; set; }
    public List<AppUserViewModel> Users { get; set; }
    public CreateProjectViewModel()
    {
        Users = new List<AppUserViewModel>();
    }
}
