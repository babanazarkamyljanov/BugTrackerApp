using BugTracker.Models.DTOs;

namespace BugTracker.Interfaces;

public interface IProjectsService
{
    Task<List<GetAllProjectDTO>> GetAll(CancellationToken cancellationToken);

    Task<GetProjectDTO> Get(Guid id, CancellationToken cancellationToken);

    CreateProjectDTO CreateGet();

    Task<CreateProjectDTO> CreatePost(CreateProjectDTO dto, CancellationToken cancellationToken);

    Task<EditProjectDTO> EditGet(Guid id, CancellationToken cancellationToken);

    Task<EditProjectDTO> EditPost(Guid id, EditProjectDTO dto, CancellationToken cancellationToken);

    Task Delete(Guid id, CancellationToken cancellationToken);
}
