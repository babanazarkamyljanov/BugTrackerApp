using BugTracker.Authorization;
using BugTracker.Interfaces;
using BugTracker.Models.DTOs;

namespace BugTracker.Services;

public class ViewsService : IViewsService
{
    private readonly IUsersService _usersService;
    private readonly UserManager<User> _userManager;
    private readonly ApplicationDbContext _context;

    public ViewsService(IUsersService usersService,
        UserManager<User> userManager,
        ApplicationDbContext context)
    {
        _usersService = usersService;
        _userManager = userManager;
        _context = context;
    }

    public async Task<List<UserDTO>> GetOrganizationManagers()
    {
        var currentUser = await _usersService.GetCurrentUserAsync();

        if (currentUser == null)
        {
            throw new ArgumentException("current logged in user wasn't found");
        }

        var organizationUsers = await _usersService
            .GetOrganizationUsersAsync(currentUser.OrganizationId);

        var managers = new List<UserDTO>();
        foreach (var user in organizationUsers)
        {
            if (await _userManager.IsInRoleAsync(user, DefaultRoles.ProjectManager))
            {
                managers.Add(new UserDTO()
                {
                    Id = user.Id,
                    Email = user.Email
                });
            }
        }

        if (managers.Count == 0)
        {
            managers.Add(new UserDTO()
            {
                Id = currentUser.Id,
                Email = currentUser.Email
            });
        }
        return managers;
    }

    public async Task<List<UserDTO>> GetOrganizationUsers()
    {
        var currentUser = await _usersService.GetCurrentUserAsync();

        if (currentUser == null)
        {
            throw new ArgumentException("current logged in user wasn't found");
        }

        var users = await _userManager.Users
            .Where(u => u.OrganizationId == currentUser.OrganizationId)
            .Select(u => new UserDTO()
            {
                Id = u.Id,
                Email = u.Email
            })
            .ToListAsync();

        return users;
    }

    public async Task<List<SharedProjectDTO>> GetOrganizationProjects()
    {
        var currentUser = await _usersService.GetCurrentUserAsync();

        if (currentUser == null)
        {
            throw new ArgumentException("current logged in user wasn't found");
        }

        var projects = await _context.Projects
            .Where(p => p.OrganizationId == currentUser.OrganizationId)
            .Select(p => new SharedProjectDTO()
            {
                Id = p.Id,
                Title = p.Title
            })
            .ToListAsync();

        return projects;
    }
}
