
using BugTracker.Authorization;

namespace BugTracker.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly IProjectRepository projectRepository;
    private readonly IAuthorizationService authorizationService;

    public ProjectsController(
        IProjectRepository projectRepository,
        IAuthorizationService authorizationService)
    {
        this.projectRepository = projectRepository;
        this.authorizationService = authorizationService;
    }

    // GET: Projects
    public async Task<IActionResult> Index()
    {
        var projects = await projectRepository.GetAll();
        return View(projects);
    }

    // this method is called by SignalR hubjs connection
    [HttpGet]
    public async Task<IActionResult> GetProjectsIndex()
    {
        var result = await projectRepository.GetAll();
        return Ok(result);
    }

    // GET: Projects/Details/5
    public async Task<IActionResult> Details(Guid id, string user, bool isRead)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.ProjectOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var project = await projectRepository.GetProject(id);
        if (project == null)
        {
            return NotFound();
        }
        return View(project);
    }

    // GET: Projects/Create
    public async Task<IActionResult> Create()
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.ProjectOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var model = await projectRepository.AddGet();
        return View(model);
    }

    // POST: Projects/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProjectViewModel model)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.ProjectOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            var result = await projectRepository.AddPost(model);
            if(result == "success")
                return RedirectToAction(nameof(Index));
            else
                Console.WriteLine(result);
        }
        return View(model);
    }

    // GET: Projects/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.ProjectOperations.Update);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var result = await projectRepository.UpdateGet(id);
        if(result == null)
        {
            return NotFound();
        }
        return View(result);
    }

    // POST: Projects/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, CreateProjectViewModel model)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.ProjectOperations.Update);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (id != model.Project.Id)
        {
            return NotFound();
        }
        if(ModelState.IsValid)
        {
            var result = await projectRepository.UpdatePost(id, model);
            if (result == "success")
                return RedirectToAction(nameof(Index));
            else if (result == "NotFound")
                return NotFound();
            else
                Console.WriteLine(result);
        }
        return View(model);
    }


    // POST: Projects/Delete/5
    [HttpPost]
    public async Task<ActionResult> DeleteConfirmed(Guid id)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.ProjectOperations.Delete);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        return Json(await projectRepository.Delete(id));
    }
}
