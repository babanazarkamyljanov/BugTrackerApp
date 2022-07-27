namespace BugTracker.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public string Message { get; set; }
    [ForeignKey("Author")]
    public string AuthorId { get; set; }
    public ApplicationUser Author { get; set; }
    [ForeignKey("Bug")]
    public int BugId { get; set; }
    public Bug Bug { get; set; }
    public DateTime CreatedDate { get; set; }

    public Comment()
    {
        CreatedDate = DateTime.Now;
    }
}
