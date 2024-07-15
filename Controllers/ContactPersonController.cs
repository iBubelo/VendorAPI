using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorAPI.DTOs;
using VendorAPI.Models;
using VendorAPI.Services;
using VendorAPI.Utilities;

namespace VendorAPI.Controllers;

[Authorize(Roles = "Admin, Manager")]
[ApiController]
[Route("api/[controller]")]
public class ContactPersonController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly RedisCacheService _redisCacheService;
    private readonly PhoneValidationService _phoneValidationService;

    public ContactPersonController(ApplicationDbContext context, IMapper mapper, RedisCacheService redisCacheService, PhoneValidationService phoneValidationService)
    {
        _context = context;
        _mapper = mapper;
        _redisCacheService = redisCacheService;
        _phoneValidationService = phoneValidationService;
    }

    /// GET: api/ContactPerson
    /// <summary>
    /// Retrieves a list of contact persons with their associated vendors.
    /// </summary>
    /// <response code="200">Returns the list of contact persons with vendors.</response>
    /// <response code="500">If an error occurs while retrieving the contact persons.</response>
    /// <returns>An <see cref="ActionResult"/> containing the list of contact persons with vendors.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContactPersonReadDto.CPWithVendor>>> GetContactPersons()
    {
        var cacheKey = "AllContactPersons";
        var cachedContactPersons = await _redisCacheService.GetAsync<List<ContactPersonReadDto.CPWithVendor>>(cacheKey);

        if (cachedContactPersons != null)
        {
            return Ok(cachedContactPersons);
        }

        var contactPersons = await _context.ContactPersons
            .Include(cp => cp.Vendor)
            .ToListAsync();

        var contactPersonDto = _mapper.Map<List<ContactPersonReadDto.CPWithVendor>>(contactPersons);

        await _redisCacheService.SetAsync(cacheKey, contactPersonDto, TimeSpan.FromMinutes(10));

        return Ok(contactPersonDto);
    }

    /// GET: api/ContactPerson/1
    /// <summary>
    /// Retrieves a contact person with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the contact person to retrieve.</param>
    /// <response code="200">Returns the contact person with the specified ID.</response>
    /// <response code="404">If the contact person with the specified ID is not found.</response>
    /// <response code="500">If an error occurs while retrieving the contact person.</response>
    /// <returns>
    /// Returns an <see cref="ActionResult{T}"/> containing the contact person with the specified ID,
    /// or a <see cref="NotFoundResult"/> if the contact person is not found.
    /// </returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ContactPersonReadDto.CPWithVendor>> GetContactPerson(int id)
    {
        var cacheKey = $"ContactPerson_{id}";
        var cachedContactPerson = await _redisCacheService.GetAsync<ContactPersonReadDto.CPWithVendor>(cacheKey);

        if (cachedContactPerson != null)
        {
            return Ok(cachedContactPerson);
        }

        var contactPerson = await _context.ContactPersons
            .Include(cp => cp.Vendor)
            .FirstOrDefaultAsync(cp => cp.Id == id);

        if (contactPerson == null)
        {
            return NotFound();
        }

        var contactPersonDto = _mapper.Map<ContactPersonReadDto.CPWithVendor>(contactPerson);

        await _redisCacheService.SetAsync(cacheKey, contactPersonDto, TimeSpan.FromMinutes(10));

        return Ok(contactPersonDto);
    }

    /// POST: api/ContactPerson
    /// <summary>
    /// Creates a new contact person.
    /// </summary>
    /// <param name="contactPersonDto">The data transfer object containing the contact person details.</param>
    /// <response code="201">Returns the created contact person.</response>
    /// <response code="400">If the contact person data is invalid.</response>
    /// <response code="401">If the user is not authorized to create a contact person.</response>
    /// <response code="403">If the user is not authorized to create a contact person.</response>
    /// <response code="404">If the vendor ID is missing or does not exist.</response>
    /// <response code="500">If an error occurs while creating the contact person.</response>
    /// <returns>An asynchronous task that represents the operation and contains the created contact person.</returns>
    [HttpPost]
    public async Task<ActionResult<ContactPersonReadDto>> CreateContactPerson(ContactPersonCreateDto contactPersonDto)
    {
        var contactPerson = _mapper.Map<ContactPerson>(contactPersonDto);

        if (!_context.Vendors.Any(v => v.Id == contactPerson.VendorId))
        {
            return BadRequest(new { VendorId = "VendorId is missing or does not exist" });
        }

        var (isValidPhoneNumber, errorMessage) = _phoneValidationService.ValidatePhoneNumber(contactPerson.Phone ?? string.Empty);
        if (!isValidPhoneNumber)
        {
            return BadRequest(new { PhoneErrorMessage = errorMessage });
        }

        contactPerson.Phone = contactPerson.Phone!.NormalizePhoneNumber();

        _context.ContactPersons.Add(contactPerson);
        await _context.SaveChangesAsync();

        var contactPersonReadDto = _mapper.Map<ContactPersonReadDto>(contactPerson);

        await _redisCacheService.RemoveAsync("AllContactPersons");

        return CreatedAtAction(nameof(GetContactPerson), new { id = contactPerson.Id }, contactPersonReadDto);
    }

    /// PUT: api/ContactPerson/1
    /// <summary>
    /// Updates a contact person with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the contact person to update.</param>
    /// <param name="contactPersonDto">The updated contact person data.</param>
    /// <response code="204">If the contact person was successfully updated.</response>
    /// <response code="400">If the ID in the URL does not match the ID in the contact person data.</response>
    /// <response code="404">If the contact person with the specified ID is not found.</response>
    /// <response code="500">If an error occurs while updating the contact person.</response>
    /// <returns>An <see cref="IActionResult"/> representing the result of the update operation.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContactPerson(int id, ContactPersonUpdateDto contactPersonDto)
    {
        if (id != contactPersonDto.Id)
        {
            return BadRequest();
        }
        if (!_context.Vendors.Any(v => v.Id == contactPersonDto.VendorId))
        {
            return BadRequest(new { VendorId = "VendorId is missing or does not exist" });
        }

        var contactPerson = await _context.ContactPersons.FindAsync(id);
        if (contactPerson == null)
        {
            return NotFound();
        }

        var (isValidPhoneNumber, errorMessage) = _phoneValidationService.ValidatePhoneNumber(contactPersonDto.Phone ?? string.Empty);
        if (!isValidPhoneNumber)
        {
            return BadRequest(new { PhoneErrorMessage = errorMessage });
        }

        contactPersonDto.Phone = contactPersonDto.Phone!.NormalizePhoneNumber();

        _mapper.Map(contactPersonDto, contactPerson);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ContactPersonExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        await _redisCacheService.RemoveAsync($"ContactPerson_{id}");
        await _redisCacheService.RemoveAsync("AllContactPersons");

        return NoContent();
    }

    /// DELETE: api/ContactPerson/1
    /// <summary>
    /// Deletes a contact person with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the contact person to delete.</param>
    /// <response code="204">If the contact person was successfully deleted.</response>
    /// <response code="404">If the contact person with the specified ID is not found.</response>
    /// <response code="500">If an error occurs while deleting the contact person.</response>
    /// <returns>An IActionResult representing the result of the deletion operation.</returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContactPerson(int id)
    {
        var contactPerson = await _context.ContactPersons.FindAsync(id);
        if (contactPerson == null)
        {
            return NotFound();
        }

        _context.ContactPersons.Remove(contactPerson);
        await _context.SaveChangesAsync();

        await _redisCacheService.RemoveAsync($"ContactPerson_{id}");
        await _redisCacheService.RemoveAsync("AllContactPersons");

        return NoContent();
    }

    private bool ContactPersonExists(int id)
    {
        return _context.ContactPersons.Any(e => e.Id == id);
    }
}
