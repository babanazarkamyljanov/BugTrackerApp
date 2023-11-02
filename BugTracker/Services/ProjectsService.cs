using BugTracker.Interfaces;
using BugTracker.Models.DTOs;
using System.Data;

namespace BugTracker.Services;

public class ProjectsService : IProjectsService
{
    private readonly ApplicationDbContext _context;
    private readonly IUsersService _usersService;
    private readonly UserManager<User> _userManager;
    private readonly IHubContext<LoadProjectsHub> _hubContext;

    public ProjectsService(ApplicationDbContext context,
        IUsersService usersService,
        UserManager<User> userManager,
        IHubContext<LoadProjectsHub> hubContext)
    {
        _context = context;
        _usersService = usersService;
        _userManager = userManager;
        _hubContext = hubContext;
    }

    public async Task<List<GetAllProjectDTO>> GetAll(CancellationToken ct)
    {
        string claim = _usersService.GetCurrentUserId();
        User? currentUser = await GetCurrentUser(claim);
        if (currentUser == null)
        {
            throw new InvalidOperationException("Current logged in user wasn't found");
        }

        List<GetAllProjectDTO> projects = await _context.Projects
            .Where(p => p.OrganizationId == currentUser.OrganizationId)
            .Select(p => new GetAllProjectDTO()
            {
                Id = p.Id,
                Title = p.Title,
                Priority = p.Priority,
                Status = p.Status,
                CreatedDate = p.CreatedDate.ToShortDateString(),
                Manager = new UserDTO()
                {
                    Email = p.Manager.Email,
                    AvatarPhoto = p.Manager.AvatarPhoto
                }
            }).ToListAsync(ct);

        return projects;
    }

    public async Task<GetProjectDTO> Get(Guid id, CancellationToken ct)
    {
        GetProjectDTO? project = await _context.Projects
            .Where(p => p.Id == id)
            .Select(p => new GetProjectDTO()
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Priority = p.Priority,
                Status = p.Status,
                CreatedDate = p.CreatedDate,
                Manager = new UserDTO()
                {
                    Email = p.Manager.Email,
                    AvatarPhoto = p.Manager.AvatarPhoto
                },
                CreatedBy = new UserDTO()
                {
                    Email = p.CreatedBy.Email,
                    AvatarPhoto = p.CreatedBy.AvatarPhoto
                },
                Bugs = p.Bugs.Select(b => new SharedBugDTO()
                {
                    Id = b.Id,
                    Title = b.Title,
                }).ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (project == null)
        {
            throw new ArgumentException("Project wasn't found");
        }

        return project;
    }

    public async Task Search(string searchTerm, CancellationToken ct)
    {
        string claim = _usersService.GetCurrentUserId();
        User? currentUser = await GetCurrentUser(claim);
        if (currentUser == null)
        {
            throw new InvalidOperationException("Current logged in user wasn't found");
        }

        List<GetAllProjectDTO> projects;
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            projects = await _context.Projects
                .Where(p => p.OrganizationId == currentUser.OrganizationId)
                .Select(p => new GetAllProjectDTO()
                {
                    Id = p.Id,
                    Title = p.Title,
                    Priority = p.Priority,
                    Status = p.Status,
                    CreatedDate = p.CreatedDate.ToShortDateString(),
                    Manager = new UserDTO()
                    {
                        Email = p.Manager.Email,
                        AvatarPhoto = p.Manager.AvatarPhoto
                    }
                }).ToListAsync(ct);
            await _hubContext.Clients.All.SendAsync("refreshProjects", projects, ct);
            return;
        }
        else
        {
            searchTerm = searchTerm.Trim().ToLower();
            projects = await _context.Projects
                .Where(p => p.OrganizationId == currentUser.OrganizationId && (
                            p.Title.ToLower().Contains(searchTerm) ||
                            p.Priority.ToLower().Contains(searchTerm) ||
                            p.Status.ToLower().Contains(searchTerm)))
                .Select(p => new GetAllProjectDTO()
                {
                    Id = p.Id,
                    Title = p.Title,
                    Priority = p.Priority,
                    Status = p.Status,
                    CreatedDate = p.CreatedDate.ToShortDateString(),
                    Manager = new UserDTO()
                    {
                        Email = p.Manager.Email,
                        AvatarPhoto = p.Manager.AvatarPhoto
                    }
                }).ToListAsync(ct);
            await _hubContext.Clients.All.SendAsync("refreshProjects", projects, ct);
            return;
        }
    }

    public async Task<CreateProjectDTO> CreatePost(CreateProjectDTO dto,
        CancellationToken ct)
    {
        // TODO validate dto
        string claim = _usersService.GetCurrentUserId();
        User? currentUser = await GetCurrentUser(claim);
        if (currentUser == null)
        {
            throw new InvalidOperationException("Current logged in user wasn't found");
        }

        Project project = new Project()
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            Priority = dto.Priority.Trim(),
            Status = dto.Status.Trim(),
            ManagerId = dto.ManagerId.Trim(),
        };

        project.CreatedById = currentUser.Id;
        project.OrganizationId = currentUser.OrganizationId;

        _context.Projects.Add(project);
        await _context.SaveChangesAsync(ct);
        return dto;

        // TODO implement notification

        // notify the user when project assigned to him
        //Notification notification = new()
        //{
        //    AssignedUserID = model.Project.ManagerId,
        //    Controller = "Projects",
        //    DetailsID = model.Project.Id.ToString(),
        //    IsRead = false
        //};
        //context.Notifications.Add(notification);
        //await context.SaveChangesAsync();

        // increment user notification count
        //var user = await _usersService.GetUserByIdAsync(model.Project.ManagerId);
        //user.NotificationCount++;
        //await _usersService.UpdateAsync(user);

        //await projectIndexHub.Clients
        //    .User(model.Project.ManagerId)
        //    .SendAsync("GetNotifications", user.NotificationCount);
        //return "success";
    }

    public async Task<EditProjectDTO> EditGet(Guid id, CancellationToken ct)
    {
        EditProjectDTO? project = await _context.Projects
            .Where(p => p.Id == id)
            .Select(p => new EditProjectDTO()
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Priority = p.Priority,
                Status = p.Status,
                ManagerId = p.ManagerId
            })
            .FirstOrDefaultAsync(ct);

        if (project == null)
        {
            throw new ArgumentException("Project by id wasn't found", nameof(id));
        }

        return project;
    }

    public async Task<EditProjectDTO> EditPost(Guid id, EditProjectDTO dto, CancellationToken ct)
    {
        if (id != dto.Id)
        {
            throw new ArgumentException("Id with project id doesn't match", nameof(id));
        }

        Project? project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            throw new ArgumentException("Project by id wasn't found", nameof(id));
        }

        project.Title = dto.Title;
        project.Description = dto.Description;
        project.Priority = dto.Priority;
        project.Status = dto.Status;
        project.ManagerId = dto.ManagerId;

        await _context.SaveChangesAsync(ct);
        return dto;
    }

    public async Task Delete(Guid id, CancellationToken ct)
    {
        Project? project = await _context.Projects
            .Where(p => p.Id == id)
            .Select(p => new Project()
            {
                Id = p.Id,
                Bugs = p.Bugs,
            })
            .FirstOrDefaultAsync(ct);

        if (project == null)
        {
            throw new ArgumentException("Project by id wasn't found", nameof(id));
        }

        if (project.Bugs != null && project.Bugs.Any())
        {
            foreach (var bug in project.Bugs)
            {

            }
            string message = "Deletion failed, project is being used by these bugs: ";
            message = string.Join(", ", project.Bugs.Select(b => b.Title));
            throw new InvalidOperationException($"{message}");
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(ct);
    }

    #region Helpers
    private async Task<User> GetCurrentUser(string claim)
    {
        User? currentUser = await _userManager.Users
            .Where(u => u.Id == claim)
            .FirstOrDefaultAsync();
        return currentUser!;
    }
    #endregion
}
