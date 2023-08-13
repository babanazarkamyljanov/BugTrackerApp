namespace BugTracker.ViewModels.ManageAccount;

public class ChangeAvatarViewModel
{
    public string Id { get; set; }
    public string AvatarPhotoPath { get; set; }
    [Required]
    [Display(Name = "Upload Avatar")]
    public IFormFile AvatarPhoto { get; set; }
    public byte[] AvatarPhotoBytes { get; set; }
}
