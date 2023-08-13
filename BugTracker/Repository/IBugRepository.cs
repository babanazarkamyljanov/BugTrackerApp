namespace BugTracker.Repository;

public interface IBugRepository
{
    IQueryable<Bug> GetAll();
    Task<Bug> Get(int id);
    Task<CreateBugViewModel> AddGet();
    Task<string> AddPost(CreateBugViewModel model);
    Task<CreateBugViewModel> UpdateGet(int id);
    Task<string> UpdatePost(int id, CreateBugViewModel model);
    Task<string> Delete(int id);
    Task<string> UploadFile(BugFile model, int id);
    Task<string> AddComment(Comment comment, int id);
    Task<CreateBugViewModel> CreateBugVM();
    Task<BugDetailsViewModel> GetBugDetails(int id);
}
