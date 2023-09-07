namespace BugTracker.Repository;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext context;
    private readonly UserManager<User> userManager;
    private readonly IHubContext<CommonHub> projectIndexHub;
    private readonly IHttpContextAccessor httpContextAccessor;

    // constructor
    public ProjectRepository(ApplicationDbContext context, IHubContext<CommonHub> projectIndexHub, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
    {
        this.context = context;
        this.projectIndexHub = projectIndexHub;
        this.userManager = userManager;
        this.httpContextAccessor = httpContextAccessor;
    }

    // get all projects
    public async Task<List<Project>> GetAll()
    {
        var userId = GetLoggedInUserId();

        List<Project> projects = await context
                                            .Projects
                                            .Include(i => i.Manager)
                                            .AsNoTracking()
                                            .ToListAsync();

        return projects;
    }

    // get project by id
    public async Task<Project> GetProject(Guid id)
    {
        var project = await context.Projects
            .Include(i => i.Bugs)
            .Include(i => i.Manager)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id);
        return project;
    }

    // add new project
    // GET
    public async Task<CreateProjectViewModel> AddGet()
    {
        var model = await CreateVM();
        return model;
    }

    // add new project
    // POST
    public async Task<string> AddPost(CreateProjectViewModel model)
    {
        try
        {
            model.Project.CreatedById = GetLoggedInUserId();
            context.Projects.Add(model.Project);
            await context.SaveChangesAsync();

            // notify the user when project assigned to him
            Notification notification = new()
            {
                AssignedUserID = model.Project.ManagerId,
                Controller = "Projects",
                DetailsID = model.Project.Id.ToString(),
                IsRead = false
            };
            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            // increment user notification count
            var user = await userManager.FindByIdAsync(model.Project.ManagerId);
            user.NotificationCount++;
            await userManager.UpdateAsync(user);

            await projectIndexHub.Clients.User(model.Project.ManagerId).SendAsync("GetNotifications", user.NotificationCount);
            return "success";
        }
        catch (Exception e)
        {
            return e.Message;
            throw;
        }
    }

    // edit project
    // GET
    public async Task<CreateProjectViewModel> UpdateGet(Guid id)
    {
        var project = await context.Projects.FindAsync(id);
        if (project == null)
        {
            return null;
        }
        var model = await CreateVM();
        model.Project = project;
        return model;
    }

    // edit project
    // POST
    public async Task<string> UpdatePost(Guid id, CreateProjectViewModel model)
    {
        try
        {
            context.Update(model.Project);
            await context.SaveChangesAsync();
            await projectIndexHub.Clients.All.SendAsync("LoadProjectsIndex");
            return "success";
        }
        catch (DbUpdateConcurrencyException e)
        {
            if (!context.Projects.Any(p => p.Id == id))
            {
                return "NotFound";
            }
            else
            {
                return e.Message;
                throw;
            }
        }
    }

    // delete project
    public async Task<string> Delete(Guid id)
    {
        var project = await GetProject(id);
        if (project == null)
        {
            return null;
        }
        try
        {
            context.Projects.Remove(project);
            await context.SaveChangesAsync();
            await projectIndexHub.Clients.All.SendAsync("LoadProjectsIndex");
            return "success";
        }
        catch (Exception e)
        {
            throw new Exception(e.ToString());
        }
    }

    // method for creating viewmodel
    private async Task<CreateProjectViewModel> CreateVM()
    {
        var model = new CreateProjectViewModel();

        // get current user
        string userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        User currentUser = userManager.Users.FirstOrDefault(u => u.Id == userId);

        // find all users within the same organization
        var organizationUsers = await userManager
                                    .Users
                                    .Where(u => u.OrganizationId == currentUser.OrganizationId)
                                    .ToListAsync();

        // add users with project manager role into managers list
        foreach (var user in organizationUsers)
        {
            if (await userManager.IsInRoleAsync(user, "Project Manager"))
            {
                model.Managers.Add(user);
            }
        }

        // if no users in manager role, just add the current user
        if (model.Managers.Count == 0)
            model.Managers.Add(currentUser);

        return model;
    }


    // get logged in user id
    private string GetLoggedInUserId()
    {
        return httpContextAccessor
                                .HttpContext
                                .User
                                .FindFirst(ClaimTypes.NameIdentifier)
                                .Value;
    }
}
