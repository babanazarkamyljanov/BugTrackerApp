namespace BugTracker.Models;

public class BugComment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string Message { get; set; } = string.Empty;

    [ForeignKey("Author")]
    public string AuthorId { get; set; } = string.Empty;
    public User Author { get; set; } = null!;

    [ForeignKey("Bug")]
    public int BugId { get; set; }
    public Bug Bug { get; set; } = null!;

    public DateTime CreatedDate { get; set; } = DateTime.Now;
}
