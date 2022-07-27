namespace BugTracker.Models;

public class BugHistory
{
    public int Id { get; set; }
    [Required]
    public string Property { get; set; }
    [Required]
    public string OldValue { get; set; }
    [Required]
    public string NewValue { get; set; }
    public DateTime DateChanged { get; set; }

    [ForeignKey("Bug")]
    public int BugId { get; set; }
    public Bug Bug { get; set; }
    public BugHistory()
    {
        DateChanged = DateTime.Now;
    }
}
