namespace BugTracker.ViewModels;

public class BugDetailsViewModel
{
    public Bug Bug { get; set; }

    // model for add comment
    public Comment Comment { get; set; }
    // model for add file to the bug
    public FileOfBug FileOfBug { get; set; }
    // this is to paginate comments of this bug
    public PaginatedList<Comment> PaginatedComments { get; set; }
    public int CommentsPageIndex { get; set; }
    public int CommentsTotalPages { get; set; }
    public PaginatedList<FileOfBug> PaginatedFiles { get; set; }
    public int FilesPageIndex { get; set; }
    public int FilesTotalPages { get; set; }
    public PaginatedList<BugHistory> PaginatedHistory { get; set; }
    public int HistoryPageIndex { get; set; }
    public int HistoryTotalPages { get; set; }
}
