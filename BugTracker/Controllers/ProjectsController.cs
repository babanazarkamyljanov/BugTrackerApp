using BugTracker.Authorization;
using BugTracker.Interfaces;
using BugTracker.Models.DTOs;

namespace BugTracker.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly IAuthorizationService authorizationService;
    private readonly IProjectsService _projectsService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(
        IAuthorizationService authorizationService,
        IProjectsService projectsService,
        ILogger<ProjectsController> logger)
    {
        this.authorizationService = authorizationService;
        _projectsService = projectsService;
        _logger = logger;
    }

    // GET: all projects
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        try
        {
            var projects = await _projectsService.GetAll(cancellationToken);
            return View(projects);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Index)}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Index)}");
            ViewData["ErrorMessage"] = "Something went wrong";
            return View();
        }
    }

    // GET: Projects/Details/5
    public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken = default)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.ProjectOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            var project = await _projectsService.Get(id, cancellationToken);
            return View(project);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Details)}");
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Details)}");
            ViewData["ErrorMessage"] = "Something went wrong";
            return View();
        }
    }

    // GET: Projects/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.ProjectOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            var dto = _projectsService.CreateGet();
            return View(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Create)}");
            ViewData["ErrorMessage"] = "Something went wrong";
            return View();
        }
    }

    // POST: Projects/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProjectDTO dto,
        CancellationToken cancellationToken = default)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.ProjectOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var result = await _projectsService.CreatePost(dto, cancellationToken);
                _logger.LogInformation("project has been created successfully", nameof(result));
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Create)}");
                NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Create)}");
                ViewData["ErrorMessage"] = ex.Message;
                return View();
            }
        }
        return View(dto);
    }

    // GET: Projects/Edit/5
    public async Task<IActionResult> Edit(Guid id, CancellationToken cancellationToken = default)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.ProjectOperations.Update);

        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            var dto = await _projectsService.EditGet(id, cancellationToken);
            return View(dto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Edit)}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Edit)}");
            ViewData["ErrorMessage"] = "Something went wrong";
            return View();
        }
    }

    // POST: Projects/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EditProjectDTO dto, CancellationToken cancellationToken = default)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.ProjectOperations.Update);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var result = await _projectsService.EditPost(id, dto, cancellationToken);
                _logger.LogInformation("project has been created successfully", nameof(result));
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Edit)}");
                ViewData["ErrorMessage"] = ex.Message;
                View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Edit)}");
                ViewData["ErrorMessage"] = "Something went wrong";
                return View(dto);
            }
        }
        return View(dto);
    }

    // POST: Projects/Delete/5
    [HttpPost]
    public async Task<ActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.ProjectOperations.Delete);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            await _projectsService.Delete(id, cancellationToken);
            _logger.LogInformation($"Project has been deleted successfully. {nameof(ProjectsController)}.{Delete}");
            return Json(new { success = true, message = "Project deleted successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{Delete}");
            return Json(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{Delete}");
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{Delete}");
            return Json(new { success = false, message = "An error occurred while deleting the project" });
        }
    }
}
