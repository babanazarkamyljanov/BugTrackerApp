using BugTracker.Authorization;
using BugTracker.Interfaces;
using BugTracker.Models.DTOs;

namespace BugTracker.Services;

public class ViewsService : IViewsService
{
    private readonly IUsersService _usersService;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly ApplicationDbContext _context;

    public ViewsService(IUsersService usersService,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        ApplicationDbContext context)
    {
        _usersService = usersService;
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<List<UserDTO>> GetOrganizationManagers()
    {
        string claim = _usersService.GetCurrentUserId();
        User? currentUser = await GetCurrentUser(claim);
        if (currentUser == null)
        {
            throw new InvalidOperationException("Current logged in user wasn't found");
        }

        List<User> organizationUsers = await _userManager.Users
            .Where(u => u.OrganizationId == currentUser.OrganizationId)
            .ToListAsync();

        List<UserDTO> managers = new List<UserDTO>();
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
        string claim = _usersService.GetCurrentUserId();
        User? currentUser = await GetCurrentUser(claim);
        if (currentUser == null)
        {
            throw new InvalidOperationException("Current logged in user wasn't found");
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

    public async Task<List<string>> GetOrganizationRoles()
    {
        string claim = _usersService.GetCurrentUserId();
        User? currentUser = await GetCurrentUser(claim);
        if (currentUser == null)
        {
            throw new InvalidOperationException("Current logged in user wasn't found");
        }
        List<string> roles = await _roleManager.Roles
            .Where(r => r.OrganizationId == currentUser.OrganizationId)
            .Select(r => r.Name)
            .ToListAsync();

        return roles;
    }

    public async Task<List<SharedProjectDTO>> GetOrganizationProjects()
    {
        string claim = _usersService.GetCurrentUserId();
        User? currentUser = await GetCurrentUser(claim);
        if (currentUser == null)
        {
            throw new InvalidOperationException("Current logged in user wasn't found");
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

    #region Helpers
    private async Task<User> GetCurrentUser(string claim)
    {
        User? currentUser = await _userManager.Users
            .Where(u => u.Id == claim)
            .FirstOrDefaultAsync();
        return currentUser!;
    }
    #endregion
}
