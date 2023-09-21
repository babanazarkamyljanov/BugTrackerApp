using BugTracker.Models.DTOs;
using BugTracker.Models.DTOs.Bug;

namespace BugTracker.Interfaces;

public interface IBugsService
{
    Task<List<GetAllBugDTO>> GetAll(CancellationToken cancellationToken);

    CreateBugDTO CreateGet();

    Task<CreateBugDTO> CreatePost(CreateBugDTO dto, CancellationToken cancellationToken);

    Task<EditBugDTO> EditGet(int id, CancellationToken cancellationToken);

    Task<EditBugDTO> EditPost(int id, EditBugDTO dto, CancellationToken cancellationToken);

    Task<BugDetailsDTO> GetDetails(int id, CancellationToken cancellationToken);

    Task<List<BugCommentDTO>> GetBugComments(int id, CancellationToken cancellationToken);

    Task<List<BugFileDTO>> GetBugFiles(int id, CancellationToken cancellationToken);

    Task UploadFile(AddFileDTO dto, CancellationToken cancellationToken);

    Task AddComment(AddCommentDTO dto, CancellationToken cancellationToken);

    Task Delete(int id, CancellationToken cancellationToken);
}
