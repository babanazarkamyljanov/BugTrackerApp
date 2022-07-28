namespace BugTracker.Repository;

public interface IProjectRepository
{
    IQueryable<Project> GetAll();
    Task<Project> Get(int id);
    CreateProjectViewModel AddGet();
    Task<string> Add(CreateProjectViewModel model);
    Task<CreateProjectViewModel> UpdateGet(int id);
    Task<string> Update(int id, CreateProjectViewModel model);
    Task<Project> Details(int id);
    Task<Project> Delete(int id);
}
