using BugTracker.Interfaces;
using BugTracker.Models.DTOs;
using System.Data;

namespace BugTracker.Services;

public class ProjectsService : IProjectsService
{
    private readonly IUsersService _usersService;
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<LoadProjectsHub> _hubContext;

    public ProjectsService(IUsersService usersService,
        ApplicationDbContext context,
        IHubContext<LoadProjectsHub> hubContext)
    {
        _usersService = usersService;
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<List<GetAllProjectDTO>> GetAll(CancellationToken ct)
    {
        var currentUser = await _usersService.GetCurrentUserAsync();

        if (currentUser == null)
        {
            throw new ArgumentException("current logged in user wasn't found");
        }

        var projects = await _context.Projects
            .Where(p => p.OrganizationId == currentUser.OrganizationId)
            .Select(p => new GetAllProjectDTO()
            {
                Id = p.Id,
                Title = p.Title,
                Key = p.Key,
                Priority = p.Priority,
                Status = p.Status,
                CreatedDate = p.CreatedDate.ToShortDateString(),
                Manager = new UserDTO()
                {
                    UserName = p.Manager.UserName,
                    AvatarPhoto = p.Manager.AvatarPhoto
                }
            }).ToListAsync(ct);

        return projects;
    }

    public async Task<GetProjectDTO> Get(Guid id, CancellationToken ct)
    {
        var project = await _context.Projects
            .Where(p => p.Id == id)
            .Select(p => new GetProjectDTO()
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                Key = p.Key,
                Priority = p.Priority,
                Status = p.Status,
                CreatedDate = p.CreatedDate,
                Manager = new UserDTO()
                {
                    UserName = p.Manager.UserName,
                    AvatarPhoto = p.Manager.AvatarPhoto
                },
                CreatedBy = new UserDTO()
                {
                    UserName = p.CreatedBy.UserName,
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
        var currentUser = await _usersService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            throw new ArgumentException("current logged in user wasn't found");
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
                    Key = p.Key,
                    Priority = p.Priority,
                    Status = p.Status,
                    CreatedDate = p.CreatedDate.ToShortDateString(),
                    Manager = new UserDTO()
                    {
                        UserName = p.Manager.UserName,
                        AvatarPhoto = p.Manager.AvatarPhoto
                    }
                }).ToListAsync(ct);
            await _hubContext.Clients.All.SendAsync("refreshProjects", projects, ct);
            return;
        }

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
                Key = p.Key,
                Priority = p.Priority,
                Status = p.Status,
                CreatedDate = p.CreatedDate.ToShortDateString(),
                Manager = new UserDTO()
                {
                    UserName = p.Manager.UserName,
                    AvatarPhoto = p.Manager.AvatarPhoto
                }
            }).ToListAsync(ct);
        await _hubContext.Clients.All.SendAsync("refreshProjects", projects, ct);
        return;
    }

    public CreateProjectDTO CreateGet()
    {
        var dto = new CreateProjectDTO();
        return dto;
    }

    public async Task<CreateProjectDTO> CreatePost(CreateProjectDTO dto,
        CancellationToken ct)
    {
        // TODO validate dto

        var project = new Project()
        {
            Title = dto.Title.Trim(),
            Key = dto.Key.Trim(),
            Description = dto.Description.Trim(),
            Priority = dto.Priority.Trim(),
            Status = dto.Status.Trim(),
            ManagerId = dto.ManagerId.Trim(),
        };
        var currentUser = await _usersService.GetCurrentUserAsync();
        if (currentUser == null)
        {
            throw new ArgumentException("Current logged in user wasn't found");
        }

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
        var project = await _context.Projects
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
            throw new ArgumentException("Project wasn't found");
        }

        return project;
    }

    public async Task<EditProjectDTO> EditPost(Guid id, EditProjectDTO dto, CancellationToken ct)
    {
        if (id != dto.Id)
        {
            throw new ArgumentException("Id with project id doesn't match");
        }

        var project = await _context.Projects.FindAsync(id);
        if (project == null)
        {
            throw new ArgumentException("Project wasn't found");
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
        var project = await _context.Projects
            .Where(p => p.Id == id)
            .Select(p => new Project()
            {
                Id = p.Id,
                Bugs = p.Bugs,
            })
            .FirstOrDefaultAsync(ct);

        if (project == null)
        {
            throw new ArgumentException("Project wasn't found");
        }

        if (project.Bugs != null && project.Bugs.Any())
        {
            foreach (var bug in project.Bugs)
            {

            }
            var message = "Deletion failed, project is being used by these bugs: ";
            message = string.Join(", ", project.Bugs.Select(b => b.Title));
            throw new InvalidOperationException($"{message}");
        }

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync(ct);
    }
}
