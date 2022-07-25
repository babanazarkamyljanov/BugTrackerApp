namespace BugTracker.Models;

public class Bug
{
    public int BugId { get; set; }

    [Required]
    [Display(Name ="Bug Title")]
    public string Title { get; set; }

    [Required]
    [Display(Name ="Bug Description")]
    public string Description { get; set; }

    [Required]
    [Display(Name ="Bug Priority")]
    public string Priority { get; set; }

    [Required]
    [Display(Name ="Bug Status")]
    public string Status { get; set; }

    [ForeignKey("Project")]
    public int ProjectId { get; set; }
    public Project Project { get; set; }

    public DateTime CreatedDate { get; set; }

    [ForeignKey("Submitter")]
    public string SubmitterId { get; set; }
    public ApplicationUser Submitter { get; set; }

    public List<BugHistory> BugHistory { get; set; }
    public List<Comment> Comments { get; set; }
    public List<FileOfBug> Files { get; set; }
    public ICollection<AppUserBug> AssignedUsersForBug { get; set; }
    public Bug()
    {
        CreatedDate = DateTime.Now;
        AssignedUsersForBug = new List<AppUserBug>();
        BugHistory = new List<BugHistory>();
        Files = new List<FileOfBug>();
    }
}
