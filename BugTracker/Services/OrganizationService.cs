using BugTracker.Interfaces;

namespace BugTracker.Services;

public class OrganizationService : IOrganizationService
{
    private readonly ApplicationDbContext _context;

    public OrganizationService(ApplicationDbContext context)
    {
        _context = context;
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
