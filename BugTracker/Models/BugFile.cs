namespace BugTracker.Models;

public class BugFile
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    [ForeignKey("Bug")]
    public int BugId { get; set; }
    public Bug Bug { get; set; } = null!;

    [NotMapped]
    [Display(Name = "Upload File")]
    public IFormFile FileHolder { get; set; } = null!;
}
