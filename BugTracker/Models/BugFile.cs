namespace BugTracker.Models;

public class BugFile
{
    public int Id { get; set; }
    public string FileName { get; set; }
    [NotMapped]
    [Display(Name ="Upload File")]
    public IFormFile File { get; set; }
    [ForeignKey("Bug")]
    public int BugId { get; set; }
    public Bug Bug { get; set; }
}
