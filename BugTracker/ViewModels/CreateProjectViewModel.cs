namespace BugTracker.ViewModels;

public class CreateProjectViewModel
{
    public Project Project { get; set; }
    //public List<AppUserViewModel> Users { get; set; }
    public List<ApplicationUser> Assignee { get; set; }
    public List<ApplicationUser> Managers { get; set; }
    public CreateProjectViewModel()
    {
        Assignee = new List<ApplicationUser>();
        Managers = new List<ApplicationUser>();
        //Users = new List<AppUserViewModel>();
    }
}
