namespace BugTracker.Repository;

public class BugRepository : IBugRepository
{
    private readonly ApplicationDbContext context;
    private readonly IHubContext<CommonHub> bugDetailsHub;
    private IHttpContextAccessor httpContextAccessor;
    private readonly IWebHostEnvironment hostEnvironment;
    private readonly UserManager<ApplicationUser> userManager;

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
    public BugRepository(ApplicationDbContext context,
        IHubContext<CommonHub> bugDetailsHub,
        IHttpContextAccessor httpContextAccessor,
        IWebHostEnvironment hostEnvironment,
        UserManager<ApplicationUser> userManager)
    {
        this.context = context;
        this.bugDetailsHub = bugDetailsHub;
        HttpContextAccessorProp = httpContextAccessor;
        this.hostEnvironment = hostEnvironment;
        this.userManager = userManager;
    }

    // get all bugs
    public IQueryable<Bug> GetAll()
    {
        return context.Bugs.Include(b => b.Project).Include(b => b.AssignedUser);
    }
    
    // get bug by id
    public async Task<Bug> Get(int id)
    {
        return await context.Bugs.FirstOrDefaultAsync(b => b.BugId == id);
    }

    // attach file to the bug
    public async Task<string> UploadFile(BugFile model, int id)
    {
        try
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
            model.BugId = id;

            //add to database and save
            context.Files.Add(model);
            await context.SaveChangesAsync();
            await bugDetailsHub.Clients.All.SendAsync("GetBugDetails");
            return "success";
        }
        catch (Exception e)
        {
            return e.Message;
            throw;
        }
    }

    // make a comment to the bug
    public async Task<string> AddComment(Comment comment, int id)
    {
        try
        {
            comment.BugId = id;
            string userId = HttpContextAccessorProp.HttpContext.
                User.FindFirst(ClaimTypes.NameIdentifier).Value;
            comment.AuthorId = userId;
            context.Comments.Add(comment);
            await context.SaveChangesAsync();
            await bugDetailsHub.Clients.All.SendAsync("GetBugDetails");
            return "success";
        }
        catch (Exception e)
        {
            return e.Message;
            throw;
        }
    }

    // Add new bug
    // GET
    public async Task<CreateBugViewModel> AddGet()
    {
        var model = await CreateBugVM();
        return model;
    }

    // Add new bug
    // POST
    public async Task<string> AddPost(CreateBugViewModel model)
    {
        try
        {
            model.Bug.Status = "Open";
            model.Bug.CreatedDate = DateTime.Now;
            string userId = HttpContextAccessorProp.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            model.Bug.SubmitterId = userId;

            context.Bugs.Add(model.Bug);
            await context.SaveChangesAsync();
            await bugDetailsHub.Clients.All.SendAsync("GetBugDetails");
            return "success";
        }
        catch (Exception e)
        {
            return e.Message;
            throw;
        }
    }

    // Edit bug
    // GET
    public async Task<CreateBugViewModel> UpdateGet(int id)
    {
        var model = await CreateBugVM();
        var bug = await context.Bugs.FindAsync(id);
        if (bug == null)
        {
            return null;
        }
        model.Bug = bug;
        return model;
    }
    
    // Edit bug
    // POST
    public async Task<string> UpdatePost(int id, CreateBugViewModel model)
    {
        var bug = await context.Bugs.AsNoTracking().FirstOrDefaultAsync(b => b.BugId == id);
        try
        {
            // Update edited Bug in the database
            context.Bugs.Update(model.Bug);
            await context.SaveChangesAsync();
            await bugDetailsHub.Clients.All.SendAsync("GetBugDetails");

            //Add changes to BugHistories table
            var currentUser = await GetCurrentUser();
            var bugHistory = new BugHistory
            {
                BugId = id,
                ChangedById = currentUser.Id
            };

            if (bug.Title != model.Bug.Title)
            {
                bugHistory.Id = 0;
                bugHistory.Property = "Title";
                bugHistory.OldValue = bug.Title;
                bugHistory.NewValue = model.Bug.Title;
                context.BugHistory.Add(bugHistory);
                await context.SaveChangesAsync();
            }
            if (bug.Description != model.Bug.Description)
            {
                bugHistory.Id = 0;
                bugHistory.Property = "Description";
                bugHistory.OldValue = bug.Description;
                bugHistory.NewValue = model.Bug.Description;
                context.BugHistory.Add(bugHistory);
                await context.SaveChangesAsync();
            }
            if (bug.Status != model.Bug.Status)
            {
                bugHistory.Id = 0;
                bugHistory.Property = "Status";
                bugHistory.OldValue = bug.Status;
                bugHistory.NewValue = model.Bug.Status;
                context.BugHistory.Add(bugHistory);
                await context.SaveChangesAsync();
            }
            if (bug.Priority != model.Bug.Priority)
            {
                bugHistory.Id = 0;
                bugHistory.Property = "Priority";
                bugHistory.OldValue = bug.Priority;
                bugHistory.NewValue = model.Bug.Priority;
                context.BugHistory.Add(bugHistory);
                await context.SaveChangesAsync();
            }
            if (bug.ProjectId != model.Bug.ProjectId)
            {
                bugHistory.Id = 0;
                bugHistory.Property = "ProjectId";
                bugHistory.OldValue = bug.ProjectId.ToString();
                bugHistory.NewValue = model.Bug.ProjectId.ToString();
                context.BugHistory.Add(bugHistory);
                await context.SaveChangesAsync();
            }
            if (bug.AssignedUserId != model.Bug.AssignedUserId)
            {
                var oldAssignee = await userManager.Users.SingleOrDefaultAsync(u => u.Id == bug.AssignedUserId);
                var newAssignee = await userManager.Users.SingleOrDefaultAsync(u => u.Id == model.Bug.AssignedUserId);
                if (oldAssignee != null && newAssignee != null)
                {
                    bugHistory.Id = 0;
                    bugHistory.Property = "Assignee";
                    bugHistory.OldValue = oldAssignee.UserName;
                    bugHistory.NewValue = newAssignee.UserName;
                    context.BugHistory.Add(bugHistory);
                    await context.SaveChangesAsync();
                }
            }
            
            await bugDetailsHub.Clients.All.SendAsync("GetBugDetails");
            return "success";
        }
        catch (DbUpdateConcurrencyException e)
        {
            if (!context.Bugs.Any(b => b.BugId == id))
            {
                return "NotFound";
            }
            else
            {
                return e.Message;
                throw;
            }
        }
    }

    // Delete bug
    public async Task<string> Delete(int id)
    {
        var bug = await Get(id);
        if(bug == null)
        {
            return null;
        }

        try
        {
            context.Bugs.Remove(bug);
            await context.SaveChangesAsync();
            return "success";
        }
        catch (Exception e)
        {
            throw new Exception(e.ToString());
        }
    }

    // return ViewModel for creation of new bug with some filled information
    public async Task<CreateBugViewModel> CreateBugVM()
    {
        var model = new CreateBugViewModel();
        string userId = httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
        ApplicationUser currentUser = await userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);

        // get users within same organization
        var organizationUsers = userManager.Users.Where(u => u.OrganizationId == currentUser.OrganizationId);
        if(organizationUsers.Any())
        {
            model.AssigneeList = organizationUsers.ToList();
        }
        else
        {
            model.AssigneeList.Add(currentUser);
        }
        model.Projects = context.Projects.ToList();
        return model;
    }

    // get bug details
    public async Task<BugDetailsViewModel> GetBugDetails(int id)
    {
        var bug = await context.Bugs
            .Include(i => i.Project)
            .Include(i => i.BugHistory)
            .ThenInclude(h => h.ChangedBy)
            .Include(i => i.AssignedUser)
            .Include(i => i.Submitter)
            .Include(i => i.Files)
            .Include(i => i.Comments)
            .ThenInclude(c => c.Author)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.BugId == id);
        if (bug == null)
        {
            return null;
        }

        var vm = new BugDetailsViewModel
        {
            Bug = bug,
            Comments = bug.Comments,
            Files = bug.Files,
            History = bug.BugHistory
        };
        return vm;
    }

    // get current user
    private async Task<ApplicationUser> GetCurrentUser()
    {
        var id = httpContextAccessor
                                .HttpContext
                                .User
                                .FindFirst(ClaimTypes.NameIdentifier)
                                .Value;
        return await userManager.Users.SingleOrDefaultAsync(u => u.Id == id);
    }
}
