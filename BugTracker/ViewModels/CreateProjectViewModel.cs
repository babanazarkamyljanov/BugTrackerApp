namespace BugTracker.ViewModels;

public class CreateProjectViewModel
{
    public Project Project { get; set; }
    //public List<AppUserViewModel> Users { get; set; }
    public List<User> Assignee { get; set; }
    public List<User> Managers { get; set; }
    public CreateProjectViewModel()
    {
        Assignee = new List<User>();
        Managers = new List<User>();
        //Users = new List<AppUserViewModel>();
    }
}
