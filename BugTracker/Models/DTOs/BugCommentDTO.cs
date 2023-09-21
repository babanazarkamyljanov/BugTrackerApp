namespace BugTracker.Models.DTOs;

public class BugCommentDTO
{
    public string Message { get; set; } = string.Empty;

    public UserDTO Author { get; set; } = new UserDTO();

    public DateTime CreatedDate { get; set; }
}
