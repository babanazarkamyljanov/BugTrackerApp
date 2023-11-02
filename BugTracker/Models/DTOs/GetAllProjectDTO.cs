namespace BugTracker.Models.DTOs;

public class GetAllProjectDTO
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string CreatedDate { get; set; } = string.Empty;

    public UserDTO Manager { get; set; } = new UserDTO();
}
