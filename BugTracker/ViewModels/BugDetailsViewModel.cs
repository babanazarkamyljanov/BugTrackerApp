namespace BugTracker.ViewModels;

public class BugDetailsViewModel
{
    public Bug Bug { get; set; }

    public Comment Comment { get; set; }

    public BugFile BugFile { get; set; }

    public List<Comment> Comments { get; set; }
    public List<BugFile> Files { get; set; }
    public List<BugHistory> History { get; set; }
}
