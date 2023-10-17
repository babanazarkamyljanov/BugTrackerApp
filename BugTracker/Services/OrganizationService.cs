using BugTracker.Interfaces;
using BugTracker.Models.DTOs;

namespace BugTracker.Services;

public class OrganizationService : IOrganizationService
{
    private readonly ApplicationDbContext _context;
    private readonly IUsersService _usersService;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;

    public OrganizationService(ApplicationDbContext context,
        IUsersService usersService,
        RoleManager<Role> roleManager,
        UserManager<User> userManager)
    {
        _context = context;
        _usersService = usersService;
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<Organization> Create(string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Must have an organization name", nameof(name));
        }

        var model = new Organization()
        {
            Name = name.Trim()
        };
        _context.Organizations.Add(model);
        await _context.SaveChangesAsync(cancellationToken);
        return model;
    }

    public async Task<GetOrganizationDTO> GetOrganization(CancellationToken ct)
    {
        var currentUser = await _usersService.GetCurrentUserAsync();

        if (currentUser == null)
        {
            throw new ArgumentException("current logged in user wasn't found");
        }

        var organization = await _context.Organizations
            .Where(o => o.Id == currentUser.OrganizationId)
            .Select(o => new GetOrganizationDTO()
            {
                Id = o.Id,
                Name = o.Name,
                Users = o.OrganizationUsers
                .Select(u => new UserDTO()
                {
                    Id = u.Id,
                    Email = u.Email,
                    UserName = u.UserName,
                    AvatarPhoto = u.AvatarPhoto,
                    Roles = _userManager.GetRolesAsync(u).Result.ToList()
                })
                .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (organization == null)
        {
            throw new ArgumentException("organization wasn't found");
        }

        return organization;
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        var organization = await _context.Organizations
            .Where(o => o.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (organization == null)
        {
            throw new ArgumentException("Organization wasn't found, deletion failed");
        }

        _context.Organizations.Remove(organization);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
