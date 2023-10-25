namespace BugTracker.Models.DTOs.Bug;

public class GetAllBugDTO
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string CreatedDate { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
    public SharedProjectDTO Project { get; set; } = new SharedProjectDTO();

    public UserDTO Assignee { get; set; } = new UserDTO();
}
