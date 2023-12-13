namespace BugTracker.Models.DTOs;

public class EditAccountDTO
{
    [Required]
    public string Id { get; set; } = string.Empty;

    public string Organization { get; set; } = string.Empty;

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(250, ErrorMessage = "The first name should have a maximum of 250 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(250, ErrorMessage = "The last name should have a maximum of 250 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string RoleName { get; set; } = string.Empty;

    [Required]
    [StringLength(15, ErrorMessage = "The phone number should have a maximum of 15 digits")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(250, ErrorMessage = "The address should have a maximum of 250 characters")]
    public string Address { get; set; } = string.Empty;

    [Required]
    public IFormFile? ProfilePhoto { get; set; }

    public byte[]? ProfilePhotoInBytes { get; set; }
}
