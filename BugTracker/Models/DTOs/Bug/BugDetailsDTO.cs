namespace BugTracker.Models.DTOs.Bug;

public class BugDetailsDTO
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; } = new DateTime();

    public SharedProjectDTO Project { get; set; } = new SharedProjectDTO();

    public UserDTO Assignee { get; set; } = new UserDTO();

    public UserDTO CreatedBy { get; set; } = new UserDTO();

    public List<BugFileDTO> Files { get; set; } = new List<BugFileDTO>();

    public List<BugCommentDTO> Comments { get; set; } = new List<BugCommentDTO>();

    public List<BugHistoryDTO> History { get; set; } = new List<BugHistoryDTO>();
}
