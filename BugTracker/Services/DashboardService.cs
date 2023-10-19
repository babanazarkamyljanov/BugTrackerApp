using BugTracker.Interfaces;
using BugTracker.Models.DTOs;

namespace BugTracker.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly IUsersService _usersService;
    private readonly UserManager<User> _userManager;

    public DashboardService(ApplicationDbContext context,
        IUsersService usersService,
        UserManager<User> userManager)
    {
        _context = context;
        _usersService = usersService;
        _userManager = userManager;

    }

    public async Task<DashboardDTO> Get(CancellationToken ct)
    {
        string claim = _usersService.GetCurrentUserId();
        User? currentUser = await _userManager.Users
            .Where(u => u.Id == claim)
            .FirstOrDefaultAsync(ct);

        if (currentUser == null)
        {
            throw new InvalidOperationException("Current logged in user wasn't found");
        }

        List<Bug> bugs = await _context.Bugs
            .Where(b => b.OrganizationId == currentUser.OrganizationId)
            .Select(b => new Bug()
            {
                Priority = b.Priority,
                Status = b.Status,
            })
            .ToListAsync(ct);

        List<Project> projects = await _context.Projects
            .Where(p => p.OrganizationId == currentUser.OrganizationId)
            .Select(p => new Project()
            {
                Priority = p.Priority,
                Status = p.Status,
            })
            .ToListAsync(ct);

        DashboardDTO dto = new DashboardDTO()
        {
            ProjectsCount = projects.Count,
            BugsCount = bugs.Count,
            UsersCount = _userManager.Users
                            .Where(u => u.OrganizationId == currentUser.OrganizationId)
                            .Count(),

            Bug_Open = bugs.Where(b => b.Status == "Open").Count(),
            Bug_BuildInProgress = bugs.Where(b => b.Status == "Build In Progress").Count(),
            Bug_CodeReview = bugs.Where(b => b.Status == "Code Review").Count(),
            Bug_FunctionalTesting = bugs.Where(b => b.Status == "Functional Testing").Count(),
            Bug_Fixed = bugs.Where(b => b.Status == "Fixed").Count(),
            Bug_Closed = bugs.Where(b => b.Status == "Closed").Count(),

            Project_Open = projects.Where(b => b.Status == "Open").Count(),
            Project_InProgress = projects.Where(b => b.Status == "In Progress").Count(),
            Project_Completed = projects.Where(b => b.Status == "Completed").Count(),
            Project_Closed = projects.Where(b => b.Status == "Closed").Count(),

            Bug_Low = bugs.Where(b => b.Priority == "Low").Count(),
            Bug_Medium = bugs.Where(b => b.Priority == "Medium").Count(),
            Bug_High = bugs.Where(b => b.Priority == "High").Count(),

            Project_Low = projects.Where(p => p.Priority == "Low").Count(),
            Project_Medium = projects.Where(p => p.Priority == "Medium").Count(),
            Project_High = projects.Where(p => p.Priority == "High").Count(),
        };

        return dto;
    }
}
