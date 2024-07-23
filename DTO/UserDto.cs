using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace VendorAPI.DTOs;

public class UserBasicDto
{
    public string? Id { get; set; }
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
}

public class UserCreateDto
{
    public string? Id { get; set; }
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }
    [JsonIgnore]
    public string? PasswordHash { get; set; }
    public string? Role { get; set; }
}

public class UserUpdateDto
{
    public string? Email { get; set; }
    public string? Role { get; set; }
}

public class UserLoginDto
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
    [Required]
    public required string Password { get; set; }
}
