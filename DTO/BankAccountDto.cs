using System.ComponentModel.DataAnnotations;

namespace VendorAPI.DTOs;

public class BankAccountCreateDto
{
    [Required]
    public required string IBAN { get; set; }
    [Required]
    public required string BIC { get; set; }
    [Required]
    public required string Name { get; set; }
    [Required]
    public int VendorId { get; set; }
}

public class BankAccountUpdateDto : BankAccountCreateDto
{   
    [Required]
    public int Id { get; set; }
}

public class BankAccountReadDto : BankAccountUpdateDto
{
    public class BAWithVendor : BankAccountReadDto
    {
        public VendorReadDto? Vendor { get; set; }
    }
}