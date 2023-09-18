namespace BugTracker.ViewModels;

public class BugDetailsViewModel
{
    public Bug Bug { get; set; } = null!;

    public BugComment Comment { get; set; } = null!;

    public BugFile File { get; set; } = null!;

    public List<BugComment> Comments { get; set; } = null!;

    public List<BugFile> Files { get; set; } = null!;

    public List<BugHistory> History { get; set; } = null!;
}
