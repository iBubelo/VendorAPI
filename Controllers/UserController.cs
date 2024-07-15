using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using VendorAPI.Models;
using VendorAPI.DTOs;

namespace VendorAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public UserController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IMapper mapper)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
    }

    /// GET: api/User
    /// <summary>
    /// Retrieves a list of users.
    /// </summary>
    /// <response code="200">Returns the list of users.</response>
    /// <response code="500">If an error occurs while retrieving the users.</response>
    /// <returns>An <see cref="ActionResult"/> containing the list of users.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserBasicDto>>> GetUsers()
    {
        return await _userManager.Users
            .Select(user => new UserBasicDto
            {
                Id = user.Id,
                Email = user.Email
            })
            .ToListAsync();
    }

    /// GET: api/User/1
    /// <summary>
    /// Retrieves a user by their ID.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <response code="200">Returns the user information.</response>
    /// <response code="404">If the user does not exist.</response>
    /// <response code="500">If an error occurs while retrieving the user.</response>
    /// <returns>An <see cref="ActionResult{T}"/> containing the user information if found, or a <see cref="NotFoundResult"/> if the user does not exist.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserBasicDto>> GetUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
    
        if (user == null)
        {
            return NotFound();
        }
    
        var userDto = new UserBasicDto
        {
            Id = user.Id,
            Email = user.Email
        };
    
        return userDto;
    }

    /// POST: api/User
    /// <summary>
    /// Creates a new user.
    /// </summary>
    /// <param name="userDto">The user data transfer object.</param>
    /// <response code="201">Returns the created user.</response>
    /// <response code="400">If the user data is invalid.</response>
    /// <returns>An <see cref="ActionResult{T}"/> containing the created user.</returns>
    [HttpPost]
    public async Task<ActionResult<UserCreateDto>> CreateUser([FromBody] UserCreateDto userDto)
    {
        var user = _mapper.Map<User>(userDto);
        user.UserName = user.Email;

        var hasher = new PasswordHasher<User>();
        user.PasswordHash = hasher.HashPassword(user, userDto.Password ?? string.Empty);

        var result = await _userManager.CreateAsync(user, user.PasswordHash);

        if (result.Succeeded)
        {
            if (!string.IsNullOrEmpty(userDto.Role))
            {
                if (await _roleManager.RoleExistsAsync(userDto.Role))
                {
                    await _userManager.AddToRoleAsync(user, userDto.Role);
                }
                else
                {
                    return BadRequest($"Role '{userDto.Role}' does not exist.");
                }
            }

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.Id },
                _mapper.Map<UserCreateDto>(user)
            );
        }
        else
        {
            return BadRequest(result.Errors);
        }
    }

    /// DELETE: api/User/1
    /// <summary>
    /// Deletes a user with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the user to delete.</param>
    /// <response code="204">If the user was successfully deleted.</response>
    /// <response code="404">If the user does not exist.</response>
    /// <response code="400">If an error occurred while deleting the user.</response>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            return NoContent();
        }
        else
        {
            return BadRequest(result.Errors);
        }
    }
}
