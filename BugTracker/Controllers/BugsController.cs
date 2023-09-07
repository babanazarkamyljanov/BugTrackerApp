using BugTracker.Authorization;

namespace BugTracker.Controllers;

[Authorize]
public class BugsController : Controller
{
    private readonly IBugRepository bugRepository;
    private readonly IAuthorizationService authorizationService;

    public BugsController(IBugRepository bugRepository, IAuthorizationService authorizationService)
    {
        this.bugRepository = bugRepository;
        this.authorizationService = authorizationService;
    }

    // GET: BugsController
    public async Task<IActionResult> Index()
    {
        var bugs = await bugRepository.GetAll().ToListAsync();
        return View(bugs);
    }

    // GET: BugsController/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.BugOperations.Read);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var bug = await bugRepository.GetBugDetails(id);
        if (bug == null)
        {
            return NotFound();
        }
        return View(bug);
    }

    // this method is called by SignalR hubjs connection
    [HttpGet]
    public async Task<IActionResult> GetBugDetails(int id)
    {
        var result = await bugRepository.GetBugDetails(id);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    // attach file to the bug
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> UploadFile(BugDetailsViewModel model, int id)
    {
        if (ModelState.IsValid)
        {
            var result = await bugRepository.UploadFile(model.BugFile, id);
            if (result == "success")
                return RedirectToAction("Details", new { @id = id });
            else
                Console.WriteLine(result);
        }
        return RedirectToAction("Details", new { @id = id });
    }

    // make a comment to the bug
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> AddComment(Comment comment, int id)
    {
        if (ModelState.IsValid)
        {
            var result = await bugRepository.AddComment(comment, id);
            if (result == "success")
                return RedirectToAction("Details", new { @id = id });
            else
                Console.WriteLine(result);
        }
        return RedirectToAction("Details", new { @id = id });
    }

    // GET: BugsController/Create
    public async Task<IActionResult> Create()
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.BugOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var result = await bugRepository.AddGet();
        return View(result);
    }

    // POST: BugsController/Create
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBugViewModel model)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.BugOperations.Create);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            var result = await bugRepository.AddPost(model);
            if (result == "success")
                return RedirectToAction(nameof(Index));
            else
                Console.WriteLine(result);
        }
        return View(model);
    }

    // GET: BugsController/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.BugOperations.Update);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        var result = await bugRepository.UpdateGet(id);
        if (result == null)
        {
            return NotFound();
        }
        return View(result);
    }

    // POST: BugsController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateBugViewModel model)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.BugOperations.Update);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        if (ModelState.IsValid)
        {
            if (id != model.Bug.Id)
            {
                return NotFound();
            }
            var result = await bugRepository.UpdatePost(id, model);
            if (result == "success")
                return RedirectToAction(nameof(Index));
            else if (result == "NotFound")
                return NotFound();
            else
                Console.WriteLine(result);
        }
        return View(model);
    }

    // Delete
    public async Task<ActionResult> DeleteConfirmed(int id)
    {
        var isAuthorized = await authorizationService.AuthorizeAsync(User, Permissions.BugOperations.Delete);
        if (!isAuthorized.Succeeded)
        {
            return RedirectToAction("AccessDenied", "Account");
        }

        return Json(await bugRepository.Delete(id));
    }
}
