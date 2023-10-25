using BugTracker.Interfaces;
using BugTracker.Models.DTOs;

namespace BugTracker.Services;

public class OrganizationService : IOrganizationService
{
    private readonly ApplicationDbContext _context;
    private readonly IUsersService _usersService;
    private readonly RoleManager<Role> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly IHubContext<LoadOrganizationHub> _loadOrganizationHubContext;

    public OrganizationService(ApplicationDbContext context,
        IUsersService usersService,
        RoleManager<Role> roleManager,
        UserManager<User> userManager,
        IHubContext<LoadOrganizationHub> loadOrganizationHubContext)
    {
        _context = context;
        _usersService = usersService;
        _roleManager = roleManager;
        _userManager = userManager;
        _loadOrganizationHubContext = loadOrganizationHubContext;
    }

    public async Task<GetOrganizationDTO> GetOrganization(CancellationToken ct)
    {
        string claim = _usersService.GetCurrentUserId();
        User? currentUser = await _userManager.Users
            .Where(u => u.Id == claim)
            .FirstOrDefaultAsync(ct);
        if (currentUser == null)
        {
            throw new InvalidOperationException("Current logged in user wasn't found");
        }

        Organization? organization = await _context.Organizations
            .Where(o => o.Id == currentUser.OrganizationId)
            .Select(o => new Organization()
            {
                Id = o.Id,
                Name = o.Name,
                OrganizationUsers = o.OrganizationUsers
            })
            .FirstOrDefaultAsync();

        if (organization == null)
        {
            throw new ArgumentException("Organization wasn't found");
        }

        GetOrganizationDTO dto = new GetOrganizationDTO()
        {
            Id = organization.Id,
            Name = organization.Name,
            Users = organization.OrganizationUsers.Select(u => new UserDTO
            {
                Id = u.Id,
                Email = u.Email,
                UserName = u.UserName,
                AvatarPhoto = u.AvatarPhoto,
                Roles = string.Join(",", _userManager.GetRolesAsync(u).Result
                                        .Select(r => r.Replace(("_" + organization.Name), "")))
            }).ToList()
        };

        return dto;
    }

    public async Task<Organization> Create(string name, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Must have an organization name", nameof(name));
        }

        if (await _context.Organizations.AnyAsync(o => o.Name == name, ct))
        {
            throw new ArgumentException("Name is already taken, try another name", nameof(name));
        }

        Organization model = new Organization()
        {
            Name = name.Trim()
        };
        _context.Organizations.Add(model);
        await _context.SaveChangesAsync(ct);
        return model;
    }

    public async Task Edit(EditOrganizationDTO dto, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new ArgumentException("Organization name can't be empty", nameof(dto.Name));
        }

        if (await _context.Organizations.AnyAsync(o => o.Name == dto.Name, ct))
        {
            throw new ArgumentException("Name is already taken, try another name", nameof(dto.Name));
        }

        Organization? organization = await _context.Organizations.FindAsync(dto.Id);
        if (organization == null)
        {
            throw new ArgumentException("Organization by id wasn't found", nameof(dto.Id));
        }

        organization.Name = dto.Name.Trim();
        _context.Organizations.Update(organization);
        await _context.SaveChangesAsync(ct);

        await _loadOrganizationHubContext.Clients.All.SendAsync("refreshOrganization", organization.Name, ct);
    }

    public async Task Delete(Guid id, CancellationToken cancellationToken)
    {
        Organization? organization = await _context.Organizations
            .Where(o => o.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        if (organization == null)
        {
            throw new ArgumentException("Organization wasn't found, deletion failed");
        }

        _context.Organizations.Remove(organization);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> IsAlreadyExists(string name, CancellationToken ct)
    {
        if (await _context.Organizations.AnyAsync(o => o.Name == name, ct))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
