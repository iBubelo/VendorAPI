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
public partial class BankAccountController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly RedisCacheService _redisCacheService;
    private readonly IbanBicValidationService _ibanBicValidationService;

    public BankAccountController(ApplicationDbContext context, IMapper mapper, RedisCacheService redisCacheService, IbanBicValidationService ibanBicValidationService)
    {
        _context = context;
        _mapper = mapper;
        _redisCacheService = redisCacheService;
        _ibanBicValidationService = ibanBicValidationService;
    }

    /// GET: api/BankAccount
    /// <summary>
    /// Retrieves a list of bank accounts with associated vendors.
    /// </summary>
    /// <response code="200">Returns the list of bank accounts with vendors.</response>
    /// <response code="500">If an error occurs while retrieving the bank accounts.</response>
    /// <returns>An <see cref="ActionResult"/> containing the list of bank accounts with vendors.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BankAccountReadDto.BAWithVendor>>> GetBankAccounts()
    {
        var cacheKey = "AllBankAccounts";
        var cachedBankAccounts = await _redisCacheService.GetAsync<List<BankAccountReadDto.BAWithVendor>>(cacheKey);

        if (cachedBankAccounts != null)
        {
            return Ok(cachedBankAccounts);
        }

        var bankAccounts = await _context.BankAccounts
            .Include(ba => ba.Vendor)
            .ToListAsync();

        var bankAccountDto = _mapper.Map<List<BankAccountReadDto.BAWithVendor>>(bankAccounts);

        await _redisCacheService.SetAsync(cacheKey, bankAccountDto, TimeSpan.FromMinutes(10));

        return Ok(bankAccountDto);
    }

    /// GET: api/BankAccount/1
    /// <summary>
    /// Retrieves a bank account with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the bank account to retrieve.</param>
    /// <response code="200">Returns the bank account with the specified ID.</response>
    /// <response code="404">If the bank account with the specified ID is not found.</response>
    /// <response code="500">If an error occurs while retrieving the bank account.</response>
    /// <returns>An <see cref="ActionResult{T}"/> containing the bank account with the specified ID.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<BankAccountReadDto.BAWithVendor>> GetBankAccount(int id)
    {
        var cacheKey = $"BankAccount:{id}";
        var cachedBankAccount = await _redisCacheService.GetAsync<BankAccountReadDto.BAWithVendor>(cacheKey);

        if (cachedBankAccount != null)
        {
            return Ok(cachedBankAccount);
        }

        var bankAccount = await _context.BankAccounts
            .Include(ba => ba.Vendor)
            .FirstOrDefaultAsync(ba => ba.Id == id);

        if (bankAccount == null)
        {
            return NotFound();
        }

        var bankAccountDto = _mapper.Map<BankAccountReadDto.BAWithVendor>(bankAccount);

        await _redisCacheService.SetAsync(cacheKey, bankAccountDto, TimeSpan.FromMinutes(10));

        return Ok(bankAccountDto);
    }

    /// POST: api/BankAccount
    /// <summary>
    /// Creates a new bank account.
    /// </summary>
    /// <param name="bankAccountDto">The bank account data to create.</param>
    /// <response code="201">Returns the newly created bank account.</response>
    /// <response code="400">If the IBAN or BIC is invalid.</response>
    /// <response code="500">If an error occurs while saving the bank account.</response>
    /// <returns>The created bank account.</returns>
    [HttpPost]
    public async Task<ActionResult<BankAccountReadDto>> CreateBankAccount(BankAccountCreateDto bankAccountDto)
    {
        var bankAccount = _mapper.Map<BankAccount>(bankAccountDto);

        // Validate IBAN and BIC
        var (isValidIban, ibanErrorMessage) = _ibanBicValidationService.ValidateIban(bankAccount.IBAN ?? string.Empty);
        var (isValidBic, bicErrorMessage) = _ibanBicValidationService.ValidateBic(bankAccount.BIC ?? string.Empty);
        if (!isValidIban || !isValidBic)
        {
            return BadRequest(new
            {
                IbanErrorMessage = ibanErrorMessage,
                BicErrorMessage = bicErrorMessage
            });
        }

        _context.BankAccounts.Add(bankAccount);
        await _context.SaveChangesAsync();

        var bankAccountReadDto = _mapper.Map<BankAccountReadDto>(bankAccount);

        await _redisCacheService.RemoveAsync("AllBankAccounts");
        

        return CreatedAtAction(nameof(GetBankAccount), new { id = bankAccount.Id }, bankAccountReadDto);
    }

    /// PUT: api/BankAccount/1
    /// <summary>
    /// Updates a bank account with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the bank account to update.</param>
    /// <param name="bankAccountDto">The updated bank account data.</param>
    /// <response code="204">If the bank account was successfully updated.</response>
    /// <response code="400">If the ID in the URL does not match the ID in the bank account data.</response>
    /// <response code="404">If the bank account with the specified ID is not found.</response>
    /// <response code="500">If an error occurs while updating the bank account.</response>
    /// <returns>An <see cref="IActionResult"/> representing the result of the update operation.</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBankAccount(int id, BankAccountUpdateDto bankAccountDto)
    {
        if (id != bankAccountDto.Id)
        {
            return BadRequest();
        }

        var bankAccount = await _context.BankAccounts.FindAsync(id);
        if (bankAccount == null)
        {
            return NotFound();
        }

        // Validate IBAN and BIC
        var (isValidIban, ibanErrorMessage) = _ibanBicValidationService.ValidateIban(bankAccountDto.IBAN);
        var (isValidBic, bicErrorMessage) = _ibanBicValidationService.ValidateBic(bankAccountDto.BIC);
        if (!isValidIban || !isValidBic)
        {
            return BadRequest(new
            {
                IbanErrorMessage = ibanErrorMessage,
                BicErrorMessage = bicErrorMessage
            });
        }

        _mapper.Map(bankAccountDto, bankAccount);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BankAccountExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        await _redisCacheService.RemoveAsync($"BankAccount:{id}");
        await _redisCacheService.RemoveAsync("AllBankAccounts");

        return NoContent();
    }

    /// DELETE: api/BankAccount/1
    /// <summary>
    /// Deletes a bank account with the specified ID.
    /// </summary>
    /// <param name="id">The ID of the bank account to delete.</param>
    /// <response code="204">If the bank account was successfully deleted.</response>
    /// <response code="404">If the bank account with the specified ID is not found.</response>
    /// <response code="500">If an error occurs while deleting the bank account.</response>
    /// <returns>An IActionResult representing the result of the deletion operation.</returns>
    /// [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBankAccount(int id)
    {
        var bankAccount = await _context.BankAccounts.FindAsync(id);
        if (bankAccount == null)
        {
            return NotFound();
        }

        _context.BankAccounts.Remove(bankAccount);
        await _context.SaveChangesAsync();

        await _redisCacheService.RemoveAsync($"BankAccount:{id}");
        await _redisCacheService.RemoveAsync("AllBankAccounts");

        return NoContent();
    }

    private bool BankAccountExists(int id)
    {
        return _context.BankAccounts.Any(e => e.Id == id);
    }
}
