using BugTracker.Models.DTOs;

namespace BugTracker.Interfaces;

public interface IProjectsService
{
    Task<List<GetAllProjectDTO>> GetAll(CancellationToken ct);

    Task<GetProjectDTO> Get(Guid id, CancellationToken ct);

    Task Search(string searchTerm, CancellationToken ct);

    Task<CreateProjectDTO> CreatePost(CreateProjectDTO dto, CancellationToken ct);

    Task<EditProjectDTO> EditGet(Guid id, CancellationToken ct);

    Task<EditProjectDTO> EditPost(Guid id, EditProjectDTO dto, CancellationToken ct);

    Task Delete(Guid id, CancellationToken ct);
}
