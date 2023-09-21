namespace BugTracker.Models.DTOs.Bug;

public class GetAllBugDTO
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string ProjectKey { get; set; } = string.Empty;

    public Guid ProjectId { get; set; }

    public UserDTO Assignee { get; set; } = new UserDTO();
}
