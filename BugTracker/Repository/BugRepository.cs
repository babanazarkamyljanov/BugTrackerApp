using BugTracker.Interfaces;

namespace BugTracker.Repository;

public class BugRepository : IBugRepository
{
    private readonly ApplicationDbContext context;
    private readonly IHubContext<CommonHub> bugDetailsHub;
    private readonly IWebHostEnvironment hostEnvironment;
    private readonly IUsersService _usersService;

    public BugRepository(ApplicationDbContext context,
        IHubContext<CommonHub> bugDetailsHub,
        IWebHostEnvironment hostEnvironment,
        IUsersService usersService)
    {
        this.context = context;
        this.bugDetailsHub = bugDetailsHub;
        this.hostEnvironment = hostEnvironment;
        _usersService = usersService;
    }

    // get all bugs
    public IQueryable<Bug> GetAll()
    {
        return context.Bugs
            .Include(b => b.Project)
            .Include(b => b.AssignedUser);
    }

    // get bug by id
    public async Task<Bug> Get(int id)
    {
        var bug = await context.Bugs.FirstOrDefaultAsync(b => b.Id == id);
        if (bug == null)
        {
            throw new ArgumentException($"Bug by id: {id} wasn't found", nameof(id));
        }
        return bug;
    }

    // attach file to the bug
    public async Task<string> UploadFile(Models.BugFile model, int id)
    {
        try
        {
            //save to wwwroot / File folder
            string UploadsFolder = Path.Combine(hostEnvironment.WebRootPath, "files");
            //unique file name
            string fileName = Path.GetFileNameWithoutExtension(model.FileHolder.FileName) +
                                DateTime.Now.ToString("yymmssfff") +
                                Path.GetExtension(model.FileHolder.FileName);

            //storing fileName to database
            model.Name = fileName;

            //copying file to wwwroot/ Files location
            string filePath = Path.Combine(UploadsFolder, fileName);
            await model.FileHolder.CopyToAsync(new FileStream(filePath, FileMode.Create));

            // sql server does inserting identity key value, so we assign id to 0
            model.BugId = id;

            //add to database and save
            context.BugFiles.Add(model);
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
    public async Task<string> AddComment(BugComment comment, int id)
    {
        try
        {
            comment.BugId = id;
            comment.AuthorId = _usersService.GetCurrentUserId();
            context.BugComments.Add(comment);
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
            var user = await _usersService.GetCurrentUserAsync();

            model.Bug.Status = "Open";
            model.Bug.CreatedDate = DateTime.Now;
            model.Bug.CreatedById = user.Id;
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
            throw new ArgumentException($"Bug by id: {id} wasn't found", nameof(id));
        }
        model.Bug = bug;
        return model;
    }

    // Edit bug
    // POST
    public async Task<string> UpdatePost(int id, CreateBugViewModel model)
    {
        var bug = await context.Bugs.AsNoTracking().FirstOrDefaultAsync(b => b.Id == id);
        if (bug == null)
        {
            throw new ArgumentException($"bug by id: {id} wasn't found", nameof(id));
        }
        try
        {
            // Update edited Bug in the database
            context.Bugs.Update(model.Bug);
            await context.SaveChangesAsync();
            await bugDetailsHub.Clients.All.SendAsync("GetBugDetails");

            //Add changes to BugHistories table
            var currentUser = await _usersService.GetCurrentUserAsync();
            var bugHistory = new BugHistory
            {
                BugId = id,
                UpdatedById = currentUser.Id
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
                var oldAssignee = await _usersService.GetUserByIdAsync(bug.AssignedUserId);
                var newAssignee = await _usersService.GetUserByIdAsync(model.Bug.AssignedUserId);
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
            if (!context.Bugs.Any(b => b.Id == id))
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
        if (bug == null)
        {
            throw new ArgumentException($"bug by id: {id} wasn't found", nameof(id));
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
        var user = await _usersService.GetCurrentUserAsync();

        var organizationUsers = await _usersService.GetOrganizationUsersAsync(user.OrganizationId);
        if (organizationUsers.Any())
        {
            model.AssigneeList = organizationUsers;
        }
        else
        {
            model.AssigneeList.Add(user);
        }
        model.Projects = context.Projects.ToList();
        return model;
    }

    // get bug details
    public async Task<BugDetailsViewModel> GetBugDetails(int id)
    {
        var bug = await context.Bugs
            .Include(i => i.Project)
            .Include(i => i.History)
            .ThenInclude(h => h.UpdatedBy)
            .Include(i => i.AssignedUser)
            .Include(i => i.CreatedBy)
            .Include(i => i.Files)
            .Include(i => i.Comments)
            .ThenInclude(c => c.Author)
            .AsSplitQuery()
            .FirstOrDefaultAsync(b => b.Id == id);
        if (bug == null)
        {
            throw new ArgumentException("Bug wasn't found", nameof(bug));
        }

        var vm = new BugDetailsViewModel
        {
            Bug = bug,
            Comments = bug.Comments.ToList(),
            Files = bug.Files.ToList(),
            History = bug.History.ToList()
        };
        return vm;
    }
}
