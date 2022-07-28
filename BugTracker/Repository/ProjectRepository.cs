namespace BugTracker.Repository;

public class ProjectRepository : IProjectRepository
{
    private readonly ApplicationDbContext context;
    private readonly UserManager<ApplicationUser> userManager;
    private readonly IHubContext<CommonHub> projectIndexHub;

    // constructor
    public ProjectRepository(ApplicationDbContext context, IHubContext<CommonHub> projectIndexHub, UserManager<ApplicationUser> userManager)
    {
        this.context = context;
        this.projectIndexHub = projectIndexHub;
        this.userManager = userManager;
    }

    // get all projects
    public IQueryable<Project> GetAll()
    {
        return context.Projects;
    }

    // get project by id
    public async Task<Project> Get(int id)
    {
        var project = await context.Projects
            .Include(i => i.AssignedUsersForProject)
                .ThenInclude(o => o.AppUser)
            .FirstOrDefaultAsync(p => p.ProjectId == id);
        return project;
    }

    // add new project
    // GET
    public CreateProjectViewModel AddGet()
    {
        var users = new List<AppUserViewModel>();
        foreach (var user in userManager.Users)
        {
            users.Add(new AppUserViewModel() { UserId = user.Id, UserEmail = user.Email, IsSelected = false });
        }
        var model = new CreateProjectViewModel
        {
            Users = users
        };
        return model;
    }

    // add new project
    // POST
    public async Task<string> Add(CreateProjectViewModel model)
    {
        try
        {
            Notification notification = new();
            context.Projects.Add(model.Project);
            await context.SaveChangesAsync();
            var id = context.Projects.Max(p => p.ProjectId);

            foreach (var user in model.Users)
            {
                if (user.IsSelected)
                {
                    // assigned users for new project
                    var appUserProject = new AppUserProject();
                    appUserProject.AppUserId = user.UserId;
                    appUserProject.ProjectId = id;
                    context.AppUserProject.Add(appUserProject);

                    // notify the developer when project assigned to him
                    notification.AssignedUserID = user.UserId;
                    notification.Controller = "Projects";
                    notification.DetailsID = id;
                    context.Notifications.Add(notification);
                }
            }
            await context.SaveChangesAsync();
            await projectIndexHub.Clients.All.SendAsync("LoadProjectsIndex");
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
    public async Task<CreateProjectViewModel> UpdateGet(int id)
    {
        var project = await context.Projects.FindAsync(id);
        if (project == null)
        {
            return null;
        }

        var users = new List<AppUserViewModel>();
        foreach (var user in userManager.Users)
        {
            var result = from item in context.AppUserProject
                         where item.AppUserId == user.Id && item.ProjectId == project.ProjectId
                         select item;

            if (result.Any())
                users.Add(new AppUserViewModel()
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    IsSelected = true
                });
            else
                users.Add(new AppUserViewModel()
                {
                    UserId = user.Id,
                    UserEmail = user.Email,
                    IsSelected = false
                });
        }
        var model = new CreateProjectViewModel
        {
            Project = project,
            Users = users
        };
        return model;
    }

    // edit project
    // POST
    public async Task<string> Update(int id, CreateProjectViewModel model)
    {
        try
        {
            context.Update(model.Project);
            foreach (var user in model.Users)
            {
                AppUserProject temp = new();
                // if user is not selected and it is already exists in the database
                // then delete it from database
                if (context.AppUserProject.Any(a => a.AppUserId == user.UserId && a.ProjectId == id) &&
                    user.IsSelected == false)
                {
                    temp.AppUserId = user.UserId;
                    temp.ProjectId = id;
                    context.AppUserProject.Remove(temp);
                }
                // if user is selected and it doesn't exist in the database
                // then add it to the database
                else if (!context.AppUserProject.Any(a => a.AppUserId == user.UserId && a.ProjectId == id) &&
                    user.IsSelected == true)
                {
                    temp.ProjectId = id;
                    temp.AppUserId = user.UserId;
                    context.AppUserProject.Add(temp);
                }
            }
            await context.SaveChangesAsync();
            await projectIndexHub.Clients.All.SendAsync("LoadProjectsIndex");
            return "success";
        }
        catch (DbUpdateConcurrencyException e)
        {
            if (!context.Projects.Any(p => p.ProjectId == id))
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

    // details of project
    public async Task<Project> Details(int id)
    {
        var project = await context.Projects
            .Include(i => i.AssignedUsersForProject)
            .ThenInclude(p => p.AppUser)
            .FirstOrDefaultAsync(m => m.ProjectId == id);
        return project;
    }

    // delete project
    public async Task<Project> Delete(int id)
    {
        var project = await Get(id);
        if (project == null)
        {
            return project;
        }
        context.AppUserProject.Remove(project.AssignedUsersForProject.FirstOrDefault());
        context.Projects.Remove(project);
        await context.SaveChangesAsync();
        await projectIndexHub.Clients.All.SendAsync("LoadProjectsIndex");
        return project;
    }
}
