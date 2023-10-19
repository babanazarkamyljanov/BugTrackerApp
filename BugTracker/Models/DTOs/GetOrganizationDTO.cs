namespace BugTracker.Models.DTOs;

public class GetOrganizationDTO
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<UserDTO> Users { get; set; } = new List<UserDTO>();
}
