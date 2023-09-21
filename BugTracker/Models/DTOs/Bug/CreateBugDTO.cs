namespace BugTracker.Models.DTOs.Bug;

public class CreateBugDTO
{
    [Required]
    [StringLength(250, ErrorMessage = "Title should have a maximum of 250 characters")]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(50, ErrorMessage = "Priority should have a maximum of 50 characters")]
    public string Priority { get; set; } = string.Empty;

    [Required]
    public string AssigneeId { get; set; } = string.Empty;

    [Required]
    public Guid ProjectId { get; set; }
}
