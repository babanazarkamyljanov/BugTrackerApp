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

    public async Task<ActionResult<GetAllBugDTO>> Index(CancellationToken ct = default)
    {
        try
        {
            List<GetAllBugDTO> bugs = await _bugsService.GetAll(ct);
            return View(bugs);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Index)}");
            return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            BugDetailsDTO bug = await _bugsService.GetDetails(id, ct);
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
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    [HttpGet]
    public async Task<ActionResult> Search(string searchTerm, CancellationToken ct = default)
    {
        try
        {
            await _bugsService.Search(searchTerm, ct);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Search)}");
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Search)}");
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetBugDetails(int id, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            BugDetailsDTO bug = await _bugsService.GetDetails(id, ct);
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
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<BugCommentDTO>>> GetBugComments(int id, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            List<BugCommentDTO> comments = await _bugsService.GetBugComments(id, ct);
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
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<BugFileDTO>>> GetBugFiles(int id, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            List<BugFileDTO> files = await _bugsService.GetBugFiles(id, ct);
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
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

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
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
            }
        }
        return RedirectToAction("Details", new { @id = dto.BugId });
    }

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
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
            }
        }
        return RedirectToAction("Details", new { @id = dto.BugId });
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        AuthorizationResult isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        CreateBugDTO dto = _bugsService.CreateGet();
        return View(dto);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<EditBugDTO>> Create(CreateBugDTO dto, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            try
            {
                CreateBugDTO result = await _bugsService.CreatePost(dto, ct);
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Create)}");
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Create)}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
            }
        }
        return View(dto);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Update);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        try
        {
            EditBugDTO dto = await _bugsService.EditGet(id, ct);
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
            return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<ActionResult<EditBugDTO>> Edit(int id, EditBugDTO dto, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Update);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            try
            {
                EditBugDTO result = await _bugsService.EditPost(id, dto, ct);
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Edit)}");
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Edit)}");
                return StatusCode(StatusCodes.Status400BadRequest, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Edit)}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong");
            }
        }
        return View(dto);
    }

    [HttpPost]
    public async Task<ActionResult> Delete(int id, CancellationToken ct = default)
    {
        AuthorizationResult isAuthorized = await authorizationService
            .AuthorizeAsync(User, Permissions.BugOperations.Delete);
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
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Delete)}");
            return Json(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Delete)}");
            return Json(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"{nameof(BugsController)}.{nameof(Delete)}");
            return Json(new { success = false, message = "An error occurred while deleting the bug" });
        }
    }
}
