namespace BugTracker.Controllers;

public class ProjectsController : Controller
{
    private readonly ApplicationDbContext context;
    private UserManager<ApplicationUser> userManager;
    private readonly IHubContext<CommonHub> projectIndexHub;

    public ProjectsController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IHubContext<CommonHub> projectIndexHub)
    {
        this.context = context;
        this.userManager = userManager;
        this.projectIndexHub = projectIndexHub;
    }

    // GET: ProjectsController
    public async Task<IActionResult> Index()
    {
        //string sortBy
        //ViewBag.SortTitle = sortBy == "Title" ? "Title desc" : "Title";
        //ViewBag.SortDate = sortBy == "Date" ? "Date desc" : "Date";

        //var projects = context.Projects.AsQueryable();
        //switch (sortBy)
        //{
        //    case "Title desc":
        //        projects = projects.OrderByDescending(p => p.Title);
        //        break;
        //    case "Title":
        //        projects = projects.OrderBy(p => p.Title);
        //        break;
        //    case "Date desc":
        //        projects = projects.OrderByDescending(p => p.CreatedDate);
        //        break;
        //    case "Date":
        //        projects = projects.OrderBy(p => p.CreatedDate);
        //        break;
        //    default:
        //        projects = projects.OrderBy(p => p.Title);
        //        break;

        //}
        return View(await context.Projects.ToListAsync());
    }

    [HttpGet]
    public IActionResult GetProjectsIndex()
    {
        var result = context.Projects.ToList();
        return Ok(result);
    }

    // GET: ProjectsController/Details/5
    public async Task<IActionResult> Details(int id,string userId, bool isRead)
    {
        var project = await context.Projects
            .Include(i => i.AssignedUsersForProject)
                .ThenInclude(p => p.AppUser)
            .FirstOrDefaultAsync(m => m.ProjectId == id);

        if (project == null)
        {
            return NotFound();
        }

        return View(project);
    }

    // GET: ProjectsController/Create
    public IActionResult Create()
    {
        var users = new List<AppUserViewModel>();
        foreach (var user in userManager.Users)
        {
            users.Add(new AppUserViewModel()
            { UserId = user.Id, UserEmail = user.Email, IsSelected = false });
        }
        var model = new CreateProjectViewModel();
        model.Users = users;
        return View(model);
    }

    // POST: ProjectsController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProjectViewModel model)
    {
        if (ModelState.IsValid)
        {
            Notification notification = new Notification();

            context.Projects.Add(model.Project);
            await context.SaveChangesAsync();
            var id = context.Projects.Max(p => p.ProjectId);

            foreach (var user in model.Users)
            {
                if(user.IsSelected)
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
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    // GET: ProjectsController/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var project = await context.Projects.FindAsync(id);
        if(project == null)
        {
            return NotFound();
        }

        var users = new List<AppUserViewModel>();
        foreach (var user in userManager.Users)
        {
            var result = from item in context.AppUserProject
                         where item.AppUserId == user.Id && item.ProjectId == project.ProjectId
                         select item;

            if(result.Any())
                users.Add(new AppUserViewModel() 
                { 
                    UserId = user.Id, UserEmail = user.Email, IsSelected = true 
                });
            else
                users.Add(new AppUserViewModel()
                { 
                    UserId = user.Id, UserEmail = user.Email, IsSelected = false 
                });
        }
        var model = new CreateProjectViewModel();
        model.Project = project;
        model.Users = users;

        return View(model);
    }

    // POST: ProjectsController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateProjectViewModel model)
    {
        if(id != model.Project.ProjectId)
        {
            return NotFound();
        }
        if(ModelState.IsValid)
        {
            try
            {
                context.Update(model.Project);

                foreach (var user in model.Users)
                {
                    AppUserProject temp = new AppUserProject();
                    // if user is not selected and it already exists in the database
                    // then delete it from table
                    if(context.AppUserProject.Any(a => a.AppUserId == user.UserId && a.ProjectId == id) &&
                        user.IsSelected == false)
                    {
                        temp.AppUserId = user.UserId;
                        temp.ProjectId = id;
                        context.AppUserProject.Remove(temp);
                    }
                    // if user is selected and it doeasn't exist in the database
                    // then add it to the table
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
            }
            catch (DbUpdateConcurrencyException)
            {
                if(!context.Projects.Any(p => p.ProjectId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    // GET: ProjectsController/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var project = await context.Projects
            .Include(i => i.AssignedUsersForProject)
                .ThenInclude(o => o.AppUser)
            .FirstOrDefaultAsync(p => p.ProjectId == id);
        if(project == null)
        {
            return NotFound();
        }

        return View(project);
    }

    // POST: ProjectsController/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var project = await context.Projects
            .Include(i => i.AssignedUsersForProject)
            .FirstOrDefaultAsync(p => p.ProjectId == id);
        if (project == null)
        {
            return NotFound();
        }

        context.AppUserProject.Remove(project.AssignedUsersForProject.FirstOrDefault());
        context.Projects.Remove(project);
        await context.SaveChangesAsync();
        await projectIndexHub.Clients.All.SendAsync("LoadProjectsIndex");
        return RedirectToAction(nameof(Index));
    }
}
