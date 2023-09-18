namespace BugTracker.Models;

public class BugHistory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Property { get; set; } = string.Empty;

    [Required]
    public string OldValue { get; set; } = string.Empty;

    [Required]
    public string NewValue { get; set; } = string.Empty;

    public DateTime UpdatedDate { get; set; } = DateTime.Now;

    [ForeignKey("UpdatedBy")]
    public string UpdatedById { get; set; } = string.Empty;
    public User UpdatedBy { get; set; } = null!;

    [ForeignKey("Bug")]
    public int BugId { get; set; }
    public Bug Bug { get; set; } = null!;
}
