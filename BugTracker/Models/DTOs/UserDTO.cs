﻿namespace BugTracker.Models.DTOs;

public class UserDTO
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Roles { get; set; } = string.Empty;

    public byte[]? AvatarPhoto { get; set; }
}
