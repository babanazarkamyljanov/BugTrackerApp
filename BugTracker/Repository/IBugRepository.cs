namespace BugTracker.Repository;

public interface IBugRepository
{
    IQueryable<Bug> GetAll();
    Task<Bug> Get(int id);
    Task<string> Add(CreateBugViewModel model);
    Task<string> Update(int id, CreateBugViewModel model);
    Task<string> UploadFile(FileOfBug model, int id);
    Task<string> AddComment(Comment comment, int id);
    Task<BugDetailsViewModel> GetVM(int id, int? pageNumberOfComments, int? pageNumberOfFiles, int? pageNumberOfHistories);
    CreateBugViewModel CreateBugVM(string view, int id);
}
