namespace BugTracker.Models.DTOs;

public class AddCommentDTO
{
    public int BugId { get; set; }

    public string Message { get; set; } = string.Empty;
}
