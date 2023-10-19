using BugTracker.Models.DTOs;

namespace BugTracker.Interfaces;

public interface IDashboardService
{
    Task<DashboardDTO> Get(CancellationToken ct);
}
