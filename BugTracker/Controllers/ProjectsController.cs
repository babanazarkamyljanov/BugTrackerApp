using BugTracker.Authorization;
using BugTracker.Interfaces;
using BugTracker.Models.DTOs;

namespace BugTracker.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IProjectsService _projectsService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(
        IAuthorizationService authorizationService,
        IProjectsService projectsService,
        ILogger<ProjectsController> logger)
    {
        _authorizationService = authorizationService;
        _projectsService = projectsService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<GetAllProjectDTO>> Index(CancellationToken ct = default)
    {
        try
        {
            List<GetAllProjectDTO> projects = await _projectsService.GetAll(ct);
            return View(projects);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Index)}");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Index)}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    [HttpGet]
    public async Task<ActionResult<GetProjectDTO>> Details(Guid id, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await _authorizationService
            .AuthorizeAsync(User, Permissions.ProjectOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            GetProjectDTO project = await _projectsService.Get(id, ct);
            return View(project);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Details)}");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Details)}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    [HttpGet]
    public async Task<ActionResult> Search(string searchTerm, CancellationToken ct = default)
    {
        try
        {
            await _projectsService.Search(searchTerm, ct);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Search)}");
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Search)}");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Search)}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    [HttpGet]
    public async Task<ActionResult<CreateProjectDTO>> Create()
    {
        AuthorizationResult isAuthorized = await _authorizationService
            .AuthorizeAsync(User, Permissions.ProjectOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        CreateProjectDTO dto = new CreateProjectDTO();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<CreateProjectDTO>> Create(CreateProjectDTO dto,
        CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await _authorizationService
            .AuthorizeAsync(User, Permissions.ProjectOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            try
            {
                CreateProjectDTO result = await _projectsService.CreatePost(dto, ct);
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Create)}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Create)}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
            }
        }
        return View(dto);
    }

    [HttpGet]
    public async Task<ActionResult<EditProjectDTO>> Edit(Guid id, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await _authorizationService
            .AuthorizeAsync(User, Permissions.ProjectOperations.Update);

        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            EditProjectDTO dto = await _projectsService.EditGet(id, ct);
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
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<EditProjectDTO>> Edit(Guid id, EditProjectDTO dto, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await _authorizationService
            .AuthorizeAsync(User, Permissions.ProjectOperations.Update);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            try
            {
                EditProjectDTO result = await _projectsService.EditPost(id, dto, ct);
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

    [HttpPost]
    public async Task<ActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await _authorizationService
            .AuthorizeAsync(User, Permissions.ProjectOperations.Delete);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            await _projectsService.Delete(id, ct);
            return Json(new { success = true, message = "Project deleted successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Delete)}");
            return Json(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Delete)}");
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(ProjectsController)}.{nameof(Delete)}");
            return Json(new { success = false, message = "An error occurred while deleting the project" });
        }
    }
}
