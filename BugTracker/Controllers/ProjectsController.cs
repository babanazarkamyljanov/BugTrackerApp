namespace BugTracker.Controllers;

public class ProjectsController : Controller
{
    private readonly IProjectRepository projectRepository;

    public ProjectsController(IProjectRepository projectRepository)
    {
        this.projectRepository = projectRepository;
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
        var projects = await projectRepository.GetAll().ToListAsync();
        return View(projects);
    }

    // this method is called by SignalR hubjs connection
    [HttpGet]
    public async Task<IActionResult> GetProjectsIndex()
    {
        var result = await projectRepository.GetAll().ToListAsync();
        return Ok(result);
    }

    // GET: ProjectsController/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var project = await projectRepository.Get(id);
        if (project == null)
        {
            return NotFound();
        }
        return View(project);
    }

    // GET: ProjectsController/Create
    public IActionResult Create()
    {
        var model = projectRepository.AddGet();
        return View(model);
    }

    // POST: ProjectsController/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProjectViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await projectRepository.Add(model);
            if(result == "success")
                return RedirectToAction(nameof(Index));
            else
                Console.WriteLine(result);
        }
        return View(model);
    }

    // GET: ProjectsController/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var result = await projectRepository.UpdateGet(id);
        if(result == null)
        {
            return NotFound();
        }
        return View(result);
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
            var result = await projectRepository.Update(id, model);
            if (result == "success")
                return RedirectToAction(nameof(Index));
            else if (result == "NotFound")
                return NotFound();
            else
                Console.WriteLine(result);
        }
        return View(model);
    }

    // GET: ProjectsController/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var project = await projectRepository.Get(id);
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
        var result = await projectRepository.Delete(id);
        if (result == null)
            return NotFound();
        return RedirectToAction(nameof(Index));
    }
}
