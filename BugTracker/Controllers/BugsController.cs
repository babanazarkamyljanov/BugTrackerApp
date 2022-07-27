using System.Security.Claims;

namespace BugTracker.Controllers;

public class BugsController : Controller
{
    private readonly ApplicationDbContext context;
    public UserManager<ApplicationUser> userManager;
    public IHttpContextAccessor httpContextAccessor;
    public IWebHostEnvironment hostEnvironment;
    private readonly IHubContext<CommonHub> bugDetailsHub;

    public BugsController(ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment hostEnvironment,
        IHubContext<CommonHub> bugDetailsHub
        )
    {
        this.context = context;
        this.userManager = userManager;
        HttpContextAccessorProp = httpContextAccessor;
        this.hostEnvironment = hostEnvironment;
        this.bugDetailsHub = bugDetailsHub;
    }
    public IHttpContextAccessor HttpContextAccessorProp
    {
        get
        {
            return this.httpContextAccessor;
        }
        set
        {
            this.httpContextAccessor = value;
        }
    }
    // GET: BugsController
    public async Task<IActionResult> Index()
    {
        var model = context.Bugs
            .Include(i => i.Project);
        return View(await model.ToListAsync());
    }

    // GET: BugsController/Details/5
    public async Task<IActionResult> Details(int id, int? pageNumberOfComments, int? pageNumberOfFiles, int? pageNumberOfHistories)
    {
        var result = await GetVM(id, pageNumberOfComments, pageNumberOfFiles, pageNumberOfHistories);
        if (result == null)
        {
            return NotFound();
        }
        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetBugDetails(int id, int? pageNumOfComments, int? pageNumOfFiles, int? pageNumOfHistories)
    {
        var result = await GetVM(id, pageNumOfComments, pageNumOfFiles, pageNumOfHistories);
        if (result == null)
        {
            return NotFound();
        }
        return Ok(result);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> UploadFile(FileOfBug model, int id)
    {
        if(ModelState.IsValid)
        {
            //save to wwwroot / File folder
            string UploadsFolder = Path.Combine(hostEnvironment.WebRootPath, "files");
            //unique file name
            string fileName = Path.GetFileNameWithoutExtension(model.File.FileName) +
                                DateTime.Now.ToString("yymmssfff") +
                                Path.GetExtension(model.File.FileName);

            //storing fileName to database
            model.FileName = fileName;

            //copying file to wwwroot/ Files location
            string filePath = Path.Combine(UploadsFolder, fileName);
            await model.File.CopyToAsync(new FileStream(filePath, FileMode.Create));

            // sql server does inserting identity key value, so we assign id to 0
            model.Id = 0;
            model.BugId = id;

            //add to database and save
            context.Files.Add(model);
            await context.SaveChangesAsync();
            await bugDetailsHub.Clients.All.SendAsync("GetBugDetails");
            return RedirectToAction("Details", new { @id = id });
        }
        return RedirectToAction("Details", new { @id = id });
    }
    

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> AddComment(Comment comment, int id)
    {
        if (ModelState.IsValid)
        {
            comment.BugId = id;
            comment.Id = 0;
            string userId = HttpContextAccessorProp.HttpContext.
                User.FindFirst(ClaimTypes.NameIdentifier).Value;
            comment.AuthorId = userId;
            context.Comments.Add(comment);
            await context.SaveChangesAsync();
            await bugDetailsHub.Clients.All.SendAsync("GetBugDetails");
            return RedirectToAction("Details", new { @id = id });
        }
        return RedirectToAction("Details", new { @id = id });
    }

    // GET: BugsController/Create
    public IActionResult Create()
    {
        var model = CreateBugVM("create", 0);
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
            string userId = HttpContextAccessorProp.HttpContext.
                User.FindFirst(ClaimTypes.NameIdentifier).Value;
            model.Bug.SubmitterId = userId;

            context.Bugs.Add(model.Bug);
            await context.SaveChangesAsync();
            var id = model.Bug.BugId;

            foreach (var user in model.Users)
            {
                if(user.IsSelected)
                {
                    var appUserBug = new AppUserBug();
                    appUserBug.AppUserId = user.UserId;
                    appUserBug.BugId = id;
                    context.AppUserBug.Add(appUserBug);
                }
            }
            await context.SaveChangesAsync();
            await bugDetailsHub.Clients.All.SendAsync("GetBugDetails");
            return RedirectToAction(nameof(Index));
        }
        return View(model);
    }

    // GET: BugsController/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var bug = await context.Bugs.FirstOrDefaultAsync(b => b.BugId == id);
        if (bug == null)
        {
            return NotFound();
        }
        var model = CreateBugVM("edit",id);
        model.Bug = bug;

        return View(model);
    }

    // POST: BugsController/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CreateBugViewModel model)
    {
        var bug = await context.Bugs.AsNoTracking().FirstOrDefaultAsync(b => b.BugId == id);

        if (id != model.Bug.BugId)
        {
            return NotFound();
        }
        if(ModelState.IsValid)
        {
            try
            {
                // Update edited Bug in the database
                context.Bugs.Update(model.Bug);
                foreach (var user in model.Users)
                {
                    AppUserBug temp = new AppUserBug();
                    // if user is not selected and it already exists in the database
                    // then delete it from table
                    if (context.AppUserBug.Any(a => a.AppUserId == user.UserId && a.BugId == id) &&
                        user.IsSelected == false)
                    {
                        temp.BugId = id;
                        temp.AppUserId = user.UserId;
                        context.AppUserBug.Remove(temp);
                    }
                    // if user is selected and it doesn't exist in the database
                    // then add it to the table
                    else if (!context.AppUserBug.Any(a => a.AppUserId == user.UserId && a.BugId == id) &&
                        user.IsSelected == true)
                    {
                        temp.BugId = id;
                        temp.AppUserId = user.UserId;
                        context.AppUserBug.Add(temp);
                    }
                }
                await context.SaveChangesAsync();
                await bugDetailsHub.Clients.All.SendAsync("GetBugDetails");

                //Add changes to BugHistories table
                var bugHistory = new BugHistory();
                bugHistory.BugId = id;
                if (bug.Title != model.Bug.Title)
                {
                    bugHistory.Property = "Title";
                    bugHistory.OldValue = bug.Title;
                    bugHistory.NewValue = model.Bug.Title;
                    context.BugHistories.Add(bugHistory);
                }
                else if(bug.Description != model.Bug.Description)
                {
                    bugHistory.Property = "Description";
                    bugHistory.OldValue = bug.Description;
                    bugHistory.NewValue = model.Bug.Description;
                    context.BugHistories.Add(bugHistory);
                }
                else if(bug.Status != model.Bug.Status)
                {
                    bugHistory.Property = "Status";
                    bugHistory.OldValue = bug.Status;
                    bugHistory.NewValue = model.Bug.Status;
                    context.BugHistories.Add(bugHistory);
                }
                else if (bug.Priority != model.Bug.Priority)
                {
                    bugHistory.Property = "Priority";
                    bugHistory.OldValue = bug.Priority;
                    bugHistory.NewValue = model.Bug.Priority;
                    context.BugHistories.Add(bugHistory);
                }
                else if (bug.ProjectId != model.Bug.ProjectId)
                {
                    bugHistory.Property = "ProjectId";
                    bugHistory.OldValue = bug.ProjectId.ToString();
                    bugHistory.NewValue = model.Bug.ProjectId.ToString();
                    context.BugHistories.Add(bugHistory);
                }
                await context.SaveChangesAsync();
                await bugDetailsHub.Clients.All.SendAsync("GetBugDetails");
            }
            catch(DbUpdateConcurrencyException)
            {
                if(!context.Bugs.Any(b => b.BugId == id))
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
    public CreateBugViewModel CreateBugVM(string view, int id)
    {
        var model = new CreateBugViewModel();
        var users = new List<AppUserViewModel>();

        if(view == "create")
        {
            foreach (var user in userManager.Users)
            {
                users.Add(new AppUserViewModel()
                { UserId = user.Id, UserEmail = user.Email, IsSelected = false });
            }
        }
        else if(view == "edit")
        {
            foreach (var user in userManager.Users)
            {
                var result = from item in context.AppUserBug
                             where item.AppUserId == user.Id && item.BugId == id
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
        }
        model.Users = users;
        model.Projects = context.Projects.ToList();
        return model;
    }
    // get BugDetailsViewModel with filled information
    private async Task<BugDetailsViewModel> GetVM(int id, int? pageNumberOfComments, int? pageNumberOfFiles, int? pageNumberOfHistories)
    {
        var bug = await context.Bugs
            .Include(i => i.Project)
            .Include(i => i.BugHistory)
            .Include(i => i.Submitter)
            .Include(i => i.AssignedUsersForBug)
            .Include(i => i.Files)
            .FirstOrDefaultAsync(b => b.BugId == id);
        if (bug == null)
        {
            return null;
        }

        int pageSize = 3;

        var comments = context.Comments.Where(c => c.BugId == id).Include(i => i.Author);
        var result = await PaginatedList<Comment>.CreateAsync(comments.AsNoTracking(), pageNumberOfComments ?? 1, pageSize);

        var files = context.Files.Where(c => c.BugId == id);
        var result2 = await PaginatedList<FileOfBug>.CreateAsync(files.AsNoTracking(), pageNumberOfFiles ?? 1, pageSize);

        var histories = context.BugHistories.Where(c => c.BugId == id);
        var result3 = await PaginatedList<BugHistory>.CreateAsync(histories.AsNoTracking(), pageNumberOfHistories ?? 1, pageSize);

        var vm = new BugDetailsViewModel
        {
            Bug = bug,
            PaginatedComments = result,
            CommentsPageIndex = result.PageIndex,
            CommentsTotalPages = result.TotalPages,
            PaginatedFiles = result2,
            FilesPageIndex = result2.PageIndex,
            FilesTotalPages = result2.TotalPages,
            PaginatedHistory = result3,
            HistoryPageIndex = result3.PageIndex,
            HistoryTotalPages = result3.TotalPages
        };
        return vm;
    }
}
