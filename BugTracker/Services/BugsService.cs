using BugTracker.Interfaces;
using BugTracker.Models.DTOs;
using BugTracker.Models.DTOs.Bug;

namespace BugTracker.Services;

public class BugsService : IBugsService
{
    private readonly IUsersService _usersService;
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<BugDetailsHub> _hubContext;
    private readonly IHubContext<LoadBugsHub> _loadBugsHubContext;
    private readonly IWebHostEnvironment _hostEnvironment;

    public BugsService(IUsersService usersService,
        ApplicationDbContext context,
        IHubContext<BugDetailsHub> hubContext,
        IHubContext<LoadBugsHub> loadBugsHubContext,
        IWebHostEnvironment hostEnvironment)
    {
        _usersService = usersService;
        _context = context;
        _hubContext = hubContext;
        _loadBugsHubContext = loadBugsHubContext;
        _hostEnvironment = hostEnvironment;
    }

    public async Task<List<GetAllBugDTO>> GetAll(CancellationToken ct)
    {
        User? currentUser = await _usersService.GetCurrentUserAsync();

        if (currentUser == null)
        {
            throw new InvalidOperationException("current logged in user wasn't found");
        }

        List<GetAllBugDTO> bugs = await _context.Bugs
            .Where(b => b.OrganizationId == currentUser.OrganizationId)
            .Select(b => new GetAllBugDTO()
            {
                Id = b.Id,
                Title = b.Title,
                CreatedDate = b.CreatedDate.ToShortDateString(),
                Priority = b.Priority,
                Status = b.Status,
                ProjectKey = b.Project.Key,
                ProjectId = b.Project.Id,
                Assignee = new UserDTO()
                {
                    UserName = b.Assignee.UserName,
                    AvatarPhoto = b.Assignee.AvatarPhoto
                }
            }).ToListAsync(ct);

        return bugs;
    }

    public async Task Search(string searchTerm, CancellationToken ct)
    {
        User? currentUser = await _usersService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            throw new InvalidOperationException("current logged in user wasn't found");
        }

        List<GetAllBugDTO> bugs;
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            bugs = await _context.Bugs
                .Where(b => b.OrganizationId == currentUser.OrganizationId)
                .Select(b => new GetAllBugDTO()
                {
                    Id = b.Id,
                    Title = b.Title,
                    CreatedDate = b.CreatedDate.ToShortDateString(),
                    Priority = b.Priority,
                    Status = b.Status,
                    ProjectKey = b.Project.Key,
                    ProjectId = b.Project.Id,
                    Assignee = new UserDTO()
                    {
                        UserName = b.Assignee.UserName,
                        AvatarPhoto = b.Assignee.AvatarPhoto
                    }
                }).ToListAsync(ct);
            await _loadBugsHubContext.Clients.All.SendAsync("refreshBugs", bugs, ct);
            return;
        }
        else
        {
            searchTerm = searchTerm.Trim().ToLower();
            bugs = await _context.Bugs
                .Where(b => b.OrganizationId == currentUser.OrganizationId && (
                            b.Title.ToLower().Contains(searchTerm) ||
                            b.Priority.ToLower().Contains(searchTerm) ||
                            b.Status.ToLower().Contains(searchTerm)))
                .Select(b => new GetAllBugDTO()
                {
                    Id = b.Id,
                    Title = b.Title,
                    CreatedDate = b.CreatedDate.ToShortDateString(),
                    Priority = b.Priority,
                    Status = b.Status,
                    ProjectKey = b.Project.Key,
                    ProjectId = b.Project.Id,
                    Assignee = new UserDTO()
                    {
                        UserName = b.Assignee.UserName,
                        AvatarPhoto = b.Assignee.AvatarPhoto
                    }
                }).ToListAsync(ct);

            await _loadBugsHubContext.Clients.All.SendAsync("refreshBugs", bugs, ct);
            return;
        }
    }

    public CreateBugDTO CreateGet()
    {
        var dto = new CreateBugDTO();
        return dto;
    }

    public async Task<CreateBugDTO> CreatePost(CreateBugDTO dto, CancellationToken ct)
    {
        Bug bug = new Bug()
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = "Open",
            ProjectId = dto.ProjectId,
            AssigneeId = dto.AssigneeId
        };

        User currentUser = await _usersService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            throw new InvalidOperationException("Current logged in user wasn't found");
        }

        bug.CreatedById = currentUser.Id;
        bug.OrganizationId = currentUser.OrganizationId;

        _context.Bugs.Add(bug);
        await _context.SaveChangesAsync(ct);

        return dto;
    }

    public async Task<EditBugDTO> EditGet(int id, CancellationToken ct)
    {
        EditBugDTO? bug = await _context.Bugs
            .Where(b => b.Id == id)
            .Select(b => new EditBugDTO()
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                Priority = b.Priority,
                Status = b.Status,
                ProjectId = b.ProjectId,
                AssigneeId = b.AssigneeId
            }).FirstOrDefaultAsync(ct);

        if (bug == null)
        {
            throw new ArgumentException("Bug by id wasn't found", nameof(id));
        }

        return bug;
    }

    public async Task<EditBugDTO> EditPost(int id, EditBugDTO dto, CancellationToken ct)
    {
        if (id != dto.Id)
        {
            throw new ArgumentException("Id with bug id doesn't match", nameof(id));
        }

        Bug? bug = await _context.Bugs.FindAsync(id);
        if (bug == null)
        {
            throw new ArgumentException("Bug by id wasn't found", nameof(id));
        }

        User? currentUser = await _usersService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            throw new InvalidOperationException("Current logged in user wasn't found");
        }

        // record changes to history table
        if (bug.Title != dto.Title)
        {
            var bugHistory = new BugHistory()
            {
                BugId = id,
                UpdatedById = currentUser.Id.ToString(),
                Property = "Title",
                OldValue = bug.Title,
                NewValue = dto.Title
            };
            _context.BugHistory.Add(bugHistory);
        }

        if (bug.Description != dto.Description)
        {
            var bugHistory = new BugHistory()
            {
                BugId = id,
                UpdatedById = currentUser.Id.ToString(),
                Property = "Description",
                OldValue = bug.Description,
                NewValue = dto.Description
            };
            _context.BugHistory.Add(bugHistory);
        }

        if (bug.Priority != dto.Priority)
        {
            var bugHistory = new BugHistory()
            {
                BugId = id,
                UpdatedById = currentUser.Id.ToString(),
                Property = "Priority",
                OldValue = bug.Priority,
                NewValue = dto.Priority
            };
            _context.BugHistory.Add(bugHistory);
        }

        if (bug.Status != dto.Status)
        {
            var bugHistory = new BugHistory()
            {
                BugId = id,
                UpdatedById = currentUser.Id.ToString(),
                Property = "Status",
                OldValue = bug.Status,
                NewValue = dto.Status
            };
            _context.BugHistory.Add(bugHistory);
        }

        if (bug.AssigneeId != dto.AssigneeId)
        {
            var bugHistory = new BugHistory()
            {
                BugId = id,
                UpdatedById = currentUser.Id.ToString(),
                Property = "AssigneeId",
                OldValue = bug.AssigneeId,
                NewValue = dto.AssigneeId
            };
            _context.BugHistory.Add(bugHistory);
        }

        if (bug.ProjectId != dto.ProjectId)
        {
            var bugHistory = new BugHistory()
            {
                BugId = id,
                UpdatedById = currentUser.Id.ToString(),
                Property = "ProjectId",
                OldValue = bug.ProjectId.ToString(),
                NewValue = dto.ProjectId.ToString()
            };
            _context.BugHistory.Add(bugHistory);
        }

        // update bug properties
        bug.Title = dto.Title;
        bug.Description = dto.Description;
        bug.Priority = dto.Priority;
        bug.Status = dto.Status;
        bug.AssigneeId = dto.AssigneeId;
        bug.ProjectId = dto.ProjectId;

        await _context.SaveChangesAsync(ct);
        return dto;
    }

    public async Task<BugDetailsDTO> GetDetails(int id, CancellationToken ct)
    {
        BugDetailsDTO? bug = await _context.Bugs
            .Where(b => b.Id == id)
            .Select(b => new BugDetailsDTO()
            {
                Id = b.Id,
                Title = b.Title,
                Priority = b.Priority,
                Status = b.Status,
                Description = b.Description,
                CreatedDate = b.CreatedDate,
                Project = new SharedProjectDTO()
                {
                    Id = b.ProjectId,
                    Title = b.Project.Title,
                },
                Assignee = new UserDTO()
                {
                    Id = b.AssigneeId,
                    UserName = b.Assignee.UserName
                },
                CreatedBy = new UserDTO()
                {
                    Id = b.CreatedById,
                    UserName = b.CreatedBy.UserName
                },
                History = b.History.Select(h => new BugHistoryDTO()
                {
                    Property = h.Property,
                    OldValue = h.OldValue,
                    NewValue = h.NewValue,
                    UpdatedBy = new UserDTO()
                    {
                        Id = h.UpdatedBy.Id,
                        UserName = h.UpdatedBy.UserName,
                    },
                    UpdatedDate = h.UpdatedDate
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (bug == null)
        {
            throw new ArgumentException("Bug by id wasn't found", nameof(id));
        }

        return bug;
    }

    public async Task<List<BugCommentDTO>> GetBugComments(int id, CancellationToken ct)
    {
        if (!_context.Bugs.Any(b => b.Id == id))
        {
            throw new ArgumentException("Bug by id wasn't found", nameof(id));
        }

        List<BugCommentDTO> comments = await _context.BugComments
            .Where(c => c.BugId == id)
            .Select(c => new BugCommentDTO()
            {
                Message = c.Message,
                Author = new UserDTO()
                {
                    UserName = c.Author.UserName,
                },
                CreatedDate = c.CreatedDate,
            }).ToListAsync(ct);

        return comments;
    }

    public async Task<List<BugFileDTO>> GetBugFiles(int id, CancellationToken ct)
    {
        if (!_context.Bugs.Any(b => b.Id == id))
        {
            throw new ArgumentException("Bug by id wasn't found", nameof(id));
        }

        List<BugFileDTO> files = await _context.BugFiles
            .Where(f => f.BugId == id)
            .Select(f => new BugFileDTO()
            {
                FileName = f.Name
            }).ToListAsync(ct);

        return files;
    }

    public async Task UploadFile(AddFileDTO dto, CancellationToken ct)
    {
        if (!_context.Bugs.Any(b => b.Id == dto.BugId))
        {
            throw new ArgumentException("Bug by id wasn't found", nameof(dto.BugId));
        }

        // get files folder path
        string UploadsFolder = Path.Combine(_hostEnvironment.WebRootPath, "bugfiles");

        // create unique file name
        string fileName = Path.GetFileNameWithoutExtension(dto.FileHolder.FileName.Trim()) +
                            DateTime.Now.ToString("yymmssfff") +
                            Path.GetExtension(dto.FileHolder.FileName);

        //storing fileName to database
        BugFile bugFile = new BugFile()
        {
            Name = fileName,
            BugId = dto.BugId
        };

        // copying file to wwwroot/ Files location
        string filePath = Path.Combine(UploadsFolder, fileName);
        await dto.FileHolder.CopyToAsync(new FileStream(filePath, FileMode.Create));

        _context.BugFiles.Add(bugFile);
        await _context.SaveChangesAsync(ct);
        await _hubContext.Clients.All.SendAsync("GetBugFiles", ct);
    }

    public async Task AddComment(AddCommentDTO dto, CancellationToken ct)
    {
        if (!_context.Bugs.Any(b => b.Id == dto.BugId))
        {
            throw new ArgumentException("Bug by id wasn't found", nameof(dto.BugId));
        }

        if (string.IsNullOrWhiteSpace(dto.Message))
        {
            throw new ArgumentException("Comment message can't empty", nameof(dto.Message));
        }

        BugComment comment = new BugComment()
        {
            BugId = dto.BugId,
            Message = dto.Message.Trim(),
            AuthorId = _usersService.GetCurrentUserId()
        };

        _context.BugComments.Add(comment);
        await _context.SaveChangesAsync(ct);
        await _hubContext.Clients.All.SendAsync("GetBugComments", ct);
    }

    public async Task Delete(int id, CancellationToken ct)
    {
        Bug? bug = await _context.Bugs
            .Include(b => b.Comments)
            .Include(b => b.History)
            .Include(b => b.Files)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (bug == null)
        {
            throw new ArgumentException("Bug by id wasn't found", nameof(id));
        }

        foreach (var comment in bug.Comments)
        {
            _context.BugComments.Remove(comment);
        }

        foreach (var history in bug.History)
        {
            _context.BugHistory.Remove(history);
        }

        // TODO implement file removing
        //foreach (var file in bug.Files)
        //{
        //}

        _context.Bugs.Remove(bug);
        await _context.SaveChangesAsync(ct);
    }
}
