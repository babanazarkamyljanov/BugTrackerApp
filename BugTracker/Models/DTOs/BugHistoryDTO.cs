namespace BugTracker.Models.DTOs;

public class BugHistoryDTO
{
    public string Property { get; set; } = string.Empty;

    public string OldValue { get; set; } = string.Empty;

    public string NewValue { get; set; } = string.Empty;

    public DateTime UpdatedDate { get; set; }

    public UserDTO UpdatedBy { get; set; } = new UserDTO();
}
