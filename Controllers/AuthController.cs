using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VendorAPI.DTOs;
using VendorAPI.Models;
using VendorAPI.Services;

namespace VendorAPI.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly JwtService _jwtService;

    public AuthController(UserManager<User> userManager, JwtService jwtService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
    }

    /// POST: api/auth/register
    /// <summary>
    /// Logs in a user with the provided credentials.
    /// </summary>
    /// <param name="userDto">The user login data.</param>
    /// <response code="200">Returns the access token.</response>
    /// <response code="401">If the user is not found or the password is incorrect.</response>
    /// <returns>An <see cref="IActionResult"/> representing the result of the login operation.</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLoginDto userDto)
    {
        var user = await _userManager.FindByEmailAsync(userDto.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, userDto.Password))
            return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);

        var accessToken = _jwtService.GenerateAccessToken(user, roles);

        return Ok(new { accessToken });
    }
    
    /// POST: api/auth/refresh-token
    /// <summary>
    /// Refreshes the access token for the authenticated user.
    /// </summary>
    /// <param name="tokensDto">The access token DTO containing the expired access token.</param>
    /// <response code="200">Returns the new access token.</response>
    /// <response code="400">If the request is invalid.</response>
    /// <returns>An <see cref="IActionResult"/> representing the response of the refresh token request.</returns>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] AccessTokenDto tokensDto)
    {
        if (tokensDto is null)
            return BadRequest("Invalid client request");

        string accessToken = tokensDto.AccessToken;

        var principal = _jwtService.GetPrincipalFromExpiredToken(accessToken);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        var user = userId != null ? await _userManager.FindByIdAsync(userId) : null;
        if (user == null)
            return BadRequest("Invalid client request");

        var roles = await _userManager.GetRolesAsync(user);

        var newAccessToken = _jwtService.GenerateAccessToken(user, roles);

        return Ok(new { newAccessToken });
    }
}
