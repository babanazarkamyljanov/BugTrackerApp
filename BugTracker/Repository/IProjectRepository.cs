namespace BugTracker.Repository;

public interface IProjectRepository
{
    Task<List<Project>> GetAll();
    Task<Project> GetProject(Guid id);
    Task<CreateProjectViewModel> AddGet();
    Task<string> AddPost(CreateProjectViewModel model);
    Task<CreateProjectViewModel> UpdateGet(Guid id);
    Task<string> UpdatePost(Guid id, CreateProjectViewModel model);
    Task<string> Delete(Guid id);
}
