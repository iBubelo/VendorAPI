using System.ComponentModel.DataAnnotations;

namespace VendorAPI.DTOs;

public class AccessTokenDto
{
    [Required]
    public required string AccessToken { get; set; }
}