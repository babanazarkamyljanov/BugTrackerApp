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

    [Display(Name ="Bug Status")]
    public string Status { get; set; }

    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    public Project Project { get; set; }

    public DateTime CreatedDate { get; set; }

    [ForeignKey("Submitter")]
    public string SubmitterId { get; set; }
    public ApplicationUser Submitter { get; set; }

    [ForeignKey("AssignedUser")]
    public string AssignedUserId { get; set; }
    public ApplicationUser AssignedUser { get; set; }

    public List<BugHistory> BugHistory { get; set; }
    public List<Comment> Comments { get; set; }
    public List<BugFile> Files { get; set; }
    //public ICollection<AppUserBug> AssignedUsersForBug { get; set; }
    public Bug()
    {
        BugHistory = new List<BugHistory>();
        Files = new List<BugFile>();
    }
}
