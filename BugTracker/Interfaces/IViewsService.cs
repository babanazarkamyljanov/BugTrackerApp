using BugTracker.Models.DTOs;

namespace BugTracker.Interfaces;

public interface IViewsService
{
    Task<List<UserDTO>> GetOrganizationManagers();

    Task<List<UserDTO>> GetOrganizationUsers();

    Task<List<SharedProjectDTO>> GetOrganizationProjects();
}
