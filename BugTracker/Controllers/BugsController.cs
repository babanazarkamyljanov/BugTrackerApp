namespace BugTracker.Controllers;

public class BugsController : Controller
{
    private readonly IBugRepository bugRepository;

    public BugsController(IBugRepository bugRepository)
    {
        this.bugRepository = bugRepository;
    }

    // GET: BugsController
    public async Task<IActionResult> Index()
    {
        var bugs = await bugRepository.GetAll().ToListAsync();
        return View(bugs);
    }

    // GET: BugsController/Details/5
    public async Task<IActionResult> Details(int id, int? pageNumberOfComments, int? pageNumberOfFiles, int? pageNumberOfHistories)
    {
        var result = await bugRepository.GetVM(id, pageNumberOfComments, pageNumberOfFiles, pageNumberOfHistories);
        if (result == null)
        {
            return NotFound();
        }
        return View(result);
    }

    // this method is called by SignalR hubjs connection
    [HttpGet]
    public async Task<IActionResult> GetBugDetails(int id, int? pageNumOfComments, int? pageNumOfFiles, int? pageNumOfHistories)
    {
        var result = await bugRepository.GetVM(id, pageNumOfComments, pageNumOfFiles, pageNumOfHistories);
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
    public async Task<IActionResult> UploadFile(FileOfBug model, int id)
    {
        if(ModelState.IsValid)
        {
            var result = await bugRepository.UploadFile(model, id);
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
    public IActionResult Create()
    {
        var model = bugRepository.CreateBugVM("create", 0);
        return View(model);
    }

    // POST: BugsController/Create
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBugViewModel model)
    {
        if(ModelState.IsValid)
        {
            var result = await bugRepository.Add(model);
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
        var bug = await bugRepository.Get(id);
        if (bug == null)
        {
            return NotFound();
        }
        var model = bugRepository.CreateBugVM("edit", id);
        model.Bug = bug;
        return View(model);
    }

    // POST: BugsController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateBugViewModel model)
    {
        if(ModelState.IsValid)
        {
            if (id != model.Bug.BugId)
            {
                return NotFound();
            }
            var result = await bugRepository.Update(id, model);
            if (result == "success")
                return RedirectToAction(nameof(Index));
            else if (result == "NotFound")
                return NotFound();
            else
                Console.WriteLine(result);
        }
        return View(model);
    }

    // GET: BugsController/Delete/5
    public ActionResult Delete(int id)
    {
        return View();
    }

    // POST: BugsController/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Delete(int id, IFormCollection collection)
    {
        try
        {
            return RedirectToAction(nameof(Index));
        }
        catch
        {
            return View();
        }
    }
}
