using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VendorAPI.DTOs;
using VendorAPI.Models;
using VendorAPI.Services;

namespace VendorAPI.Controllers;

[Authorize(Roles = "Admin, Manager")]
[ApiController]
[Route("api/[controller]")]
public class VendorController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly RedisCacheService _redisCacheService;

    public VendorController(ApplicationDbContext context, IMapper mapper, RedisCacheService redisCacheService)
    {
        _context = context;
        _mapper = mapper;
        _redisCacheService = redisCacheService;
    }

    /// GET: api/Vendor
    /// <summary>
    /// Retrieves a list of vendors with their associated bank accounts and contact persons.
    /// </summary>
    /// <response code="200">Returns the list of vendors with their associated bank accounts and contact persons.</response>
    /// <response code="500">If an error occurs while retrieving the vendors.</response>
    /// <returns>An <see cref="ActionResult"/> containing the list of vendors with their associated bank accounts and contact persons.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VendorReadDto.WithChildren>>> GetVendors()
    {
        var cacheKey = "AllVendors";
        var cachedVendors = await _redisCacheService.GetAsync<List<VendorReadDto.WithChildren>>(cacheKey);

        if (cachedVendors != null)
        {
            return Ok(cachedVendors);
        }

        var vendors = await _context.Vendors
                .Include(v => v.BankAccounts)
                .Include(v => v.ContactPersons)
                .ToListAsync();

        var vendorDtos = _mapper.Map<List<VendorReadDto.WithChildren>>(vendors);

        await _redisCacheService.SetAsync(cacheKey, vendorDtos, TimeSpan.FromMinutes(10));

        return Ok(vendorDtos);
    }

    /// GET: api/Vendor/1
    /// <summary>
    /// Retrieves a vendor with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the vendor to retrieve.</param>
    /// <response code="200">Returns the vendor information.</response>
    /// <response code="404">If the vendor is not found.</response>
    /// <response code="500">If an error occurs while retrieving the vendor.</response>
    /// <returns>An <see cref="ActionResult{T}"/> containing the vendor information.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<VendorReadDto.WithChildren>> GetVendor(int id)
    {
        var cacheKey = $"Vendor:{id}";
        var cachedVendor = await _redisCacheService.GetAsync<VendorReadDto.WithChildren>(cacheKey);

        if (cachedVendor != null)
        {
            return Ok(cachedVendor);
        }

        var vendor = await _context.Vendors
            .Include(v => v.BankAccounts)
            .Include(v => v.ContactPersons)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vendor == null)
        {
            return NotFound();
        }

        var vendorDto = _mapper.Map<VendorReadDto.WithChildren>(vendor);

        await _redisCacheService.SetAsync(cacheKey, vendorDto, TimeSpan.FromMinutes(10));

        return Ok(vendorDto);
    }

    /// POST: api/Vendor
    /// <summary>
    /// Creates a new vendor with the provided vendor data.
    /// </summary>
    /// <param name="vendorCreateDto">The data for creating the vendor.</param>
    /// <returns>The created vendor with its related entities.</returns>
    /// <response code="201">Returns the newly created vendor with its related entities.</response>
    /// <response code="400">If the vendor data is invalid.</response>
    /// <response code="500">If an error occurred while saving the vendor.</response>
    /// <remarks>
    /// Sample request:
    /// 
    ///     POST api/Vendor
    ///     {
    ///         "name": "Global Innovations Ltd",
    ///         "name2": "GI Ltd",
    ///         "address1": "456 Tech Boulevard",
    ///         "address2": "Floor 7",
    ///         "zip": "SW1A 1AA",
    ///         "country": "United Kingdom",
    ///         "city": "London",
    ///         "mail": "contact@globalinnovations.com",
    ///         "phone": "+44 20 7123 4567",
    ///         "notes": "Specialized in AI solutions",
    ///         "bankAccounts": [
    ///           {
    ///             "iban": "GB33 BUKB 2020 1555 5555 55",
    ///             "bic": "BUKBGB22",
    ///             "name": "GI Main Account"
    ///           }
    ///         ],
    ///         "contactPersons": [
    ///           {
    ///             "firstName": "Emma",
    ///             "lastName": "Watson",
    ///             "phone": "+44 20 7234 5678",
    ///             "mail": "emma.watson@globalinnovations.com"
    ///           }
    ///         ]
    ///     }
    /// 
    /// </remarks>
    [HttpPost]
    public async Task<ActionResult<VendorReadDto.WithChildren>> CreateVendor(VendorCreateDto vendorCreateDto)
    {
        var vendor = _mapper.Map<Vendor>(vendorCreateDto);

        _context.Vendors.Add(vendor);
        await _context.SaveChangesAsync();

        // Reload the vendor with its related entities
        await _context.Entry(vendor)
            .Collection(v => v.BankAccounts ?? new List<BankAccount>())
            .LoadAsync();
        await _context.Entry(vendor)
            .Collection(v => v.ContactPersons ?? new List<ContactPerson>())
            .LoadAsync();

        var vendorReadDto = _mapper.Map<VendorReadDto.WithChildren>(vendor);

        await _redisCacheService.RemoveAsync("AllVendors");
        await _redisCacheService.RemoveAsync("AllBankAccounts");
        await _redisCacheService.RemoveAsync("AllContactPersons");

        return CreatedAtAction(nameof(GetVendor), new { id = vendor.Id }, vendorReadDto);
    }

    /// PUT: api/Vendor/1
    /// <summary>
    /// Updates a vendor with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the vendor to update.</param>
    /// <param name="vendorDto">The updated vendor data.</param>
    /// <response code="204">If the vendor was successfully updated.</response>
    /// <response code="400">If the ID in the URL does not match the ID in the vendor data.</response>
    /// <response code="404">If the vendor is not found.</response>
    /// <response code="500">If an error occurred while updating the vendor.</response>
    /// <returns>An <see cref="IActionResult"/> representing the result of the update operation.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateVendor(int id, VendorReadDto vendorDto)
    {
        if (id != vendorDto.Id)
        {
            return BadRequest();
        }

        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
        {
            return NotFound();
        }

        _mapper.Map(vendorDto, vendor);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!VendorExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        await _redisCacheService.RemoveAsync($"Vendor:{id}");
        await _redisCacheService.RemoveAsync("AllVendors");

        return NoContent();
    }

    /// DELETE: api/Vendor/1
    /// <summary>
    /// Deletes a vendor with the specified ID.
    /// Delete behavior cascade is set on the BankAccounts and ContactPersons relationships.
    /// </summary>
    /// <param name="id">The ID of the vendor to delete.</param>
    /// <response code="204">If the vendor was successfully deleted.</response>
    /// <response code="404">If the vendor is not found.</response>
    /// <response code="500">If an error occurred while deleting the vendor.</response>
    /// <returns>An IActionResult representing the result of the delete operation.</returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVendor(int id)
    {
        var vendor = await _context.Vendors.FindAsync(id);
        if (vendor == null)
        {
            return NotFound();
        }

        _context.Vendors.Remove(vendor);
        await _context.SaveChangesAsync();

        await _redisCacheService.RemoveAsync($"Vendor:{id}");
        await _redisCacheService.RemoveAsync("AllVendors");

        return NoContent();
    }

    private bool VendorExists(int id)
    {
        return _context.Vendors.Any(e => e.Id == id);
    }
}
