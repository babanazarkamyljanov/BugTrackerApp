using BugTracker.Authorization;
using BugTracker.Interfaces;
using BugTracker.Models.DTOs;
using BugTracker.Models.DTOs.Bug;

namespace BugTracker.Controllers;

[Authorize]
public class BugsController : Controller
{
    private readonly IBugsService _bugsService;
    private readonly IAuthorizationService authorizationService;
    private readonly ILogger<ProjectsController> _logger;

    public BugsController(IBugsService bugsService,
        IAuthorizationService authorizationService,
        ILogger<ProjectsController> logger)
    {
        _bugsService = bugsService;
        this.authorizationService = authorizationService;
        _logger = logger;
    }

    // GET: BugsController
    public async Task<IActionResult> Index(CancellationToken ct = default)
    {
        var bugs = await _bugsService.GetAll(ct);
        return View(bugs);
    }

    // GET: BugsController/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            var bug = await _bugsService.GetDetails(id, ct);
            return View(bug);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Details)}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Details)}");
            ViewData["ErrorMessage"] = "Something went wrong";
            return View();
        }
    }

    // GET: Search for projects
    [HttpGet]
    public async Task<ActionResult> Search(string searchTerm, CancellationToken ct = default)
    {
        try
        {
            await _bugsService.Search(searchTerm, ct);
            return Ok();
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Search)}");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Search)}");
            return StatusCode(500, "Something went wrong");
        }
    }

    // this method is called by SignalR hubjs connection
    [HttpGet]
    public async Task<IActionResult> GetBugDetails(int id, CancellationToken ct = default)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            var bug = await _bugsService.GetDetails(id, ct);
            return Ok(bug);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Details)}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Details)}");
            ViewData["ErrorMessage"] = "Something went wrong";
            return View();
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<BugCommentDTO>>> GetBugComments(int id, CancellationToken ct = default)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            var comments = await _bugsService.GetBugComments(id, ct);
            return Ok(comments);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(GetBugComments)}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(GetBugComments)}");
            ViewData["ErrorMessage"] = "Something went wrong";
            return View();
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<BugFileDTO>>> GetBugFiles(int id, CancellationToken ct = default)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            var files = await _bugsService.GetBugFiles(id, ct);
            return Ok(files);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(GetBugFiles)}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(GetBugFiles)}");
            ViewData["ErrorMessage"] = "Something went wrong";
            return View();
        }
    }

    // attach file to bug
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> UploadFile(AddFileDTO dto, CancellationToken ct = default)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _bugsService.UploadFile(dto, ct);
                _logger.LogInformation("File has been uploaded successfully");
                return RedirectToAction("Details", new { @id = dto.BugId });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(UploadFile)}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(UploadFile)}");
                ViewData["ErrorMessage"] = ex.Message;
                return View();
            }
        }
        return RedirectToAction("Details", new { @id = dto.BugId });
    }

    // add a comment to bug
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> AddComment(AddCommentDTO dto, CancellationToken ct = default)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _bugsService.AddComment(dto, ct);
                _logger.LogInformation("Comment has been added successfully");
                return RedirectToAction("Details", new { @id = dto.BugId });
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(AddComment)}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(AddComment)}");
                ViewData["ErrorMessage"] = ex.Message;
                return View();
            }
        }
        return RedirectToAction("Details", new { @id = dto.BugId });
    }

    // GET: BugsController/Create
    public async Task<IActionResult> Create()
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var dto = _bugsService.CreateGet();
        return View(dto);
    }

    // POST: BugsController/Create
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBugDTO dto, CancellationToken ct)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var result = await _bugsService.CreatePost(dto, ct);
                _logger.LogInformation("project has been created successfully", nameof(result));
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Create)}");
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Create)}");
                ViewData["ErrorMessage"] = ex.Message;
                return View();
            }
        }
        return View(dto);
    }

    // GET: BugsController/Edit/5
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Update);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            var dto = await _bugsService.EditGet(id, ct);
            return View(dto);
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Edit)}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Edit)}");
            ViewData["ErrorMessage"] = "Something went wrong";
            return View();
        }
    }

    // POST: BugsController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditBugDTO dto, CancellationToken ct = default)
    {
        var isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Update);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var result = await _bugsService.EditPost(id, dto, ct);
                _logger.LogInformation("project has been created successfully", nameof(result));
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Edit)}");
                ViewData["ErrorMessage"] = ex.Message;
                View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Edit)}");
                ViewData["ErrorMessage"] = "Something went wrong";
                return View(dto);
            }
        }
        return View(dto);
    }

    // Delete: BugsController/Delete/5
    [HttpPost]
    public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.BugOperations.Delete);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            await _bugsService.Delete(id, ct);
            return Json(new { success = true, message = "Bug deleted successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{Delete}");
            return Json(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{Delete}");
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{Delete}");
            return Json(new { success = false, message = "An error occurred while deleting the bug" });
        }
    }
}
