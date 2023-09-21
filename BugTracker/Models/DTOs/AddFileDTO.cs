using System.ComponentModel;

namespace BugTracker.Models.DTOs;

public class AddFileDTO
{
    public int BugId { get; set; }

    [DisplayName("File")]
    public IFormFile FileHolder { get; set; } = null!;
}
