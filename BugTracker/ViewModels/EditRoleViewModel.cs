namespace BugTracker.ViewModels;

public class EditRoleViewModel
{
    public string Id { get; set; }
    [Required(ErrorMessage ="Role Name id required")]
    public string RoleName { get; set; }
    public List<string> Users { get; set; }

    public EditRoleViewModel()
    {
        Users = new List<string>();
    }
}
