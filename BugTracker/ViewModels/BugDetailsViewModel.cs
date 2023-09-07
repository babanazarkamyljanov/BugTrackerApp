namespace BugTracker.ViewModels;

public class BugDetailsViewModel
{
    public Bug Bug { get; set; }

    public Comment Comment { get; set; }

    public Models.File BugFile { get; set; }

    public List<Comment> Comments { get; set; }
    public List<Models.File> Files { get; set; }
    public List<History> History { get; set; }
}
