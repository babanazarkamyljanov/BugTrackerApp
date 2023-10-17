﻿namespace BugTracker.Models.DTOs;

public class UserDTO
{
    public string Id { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new List<string>();

    public byte[]? AvatarPhoto { get; set; }
}
