using System.ComponentModel.DataAnnotations;

namespace VendorAPI.DTOs;

public class VendorCreateDto
{
    [Required]
    public string? Name { get; set; }
    public string? Name2 { get; set; }
    [Required]
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? ZIP { get; set; }
    [Required]
    public string? Country { get; set; }
    public string? City { get; set; }
    [Required]
    public string? Mail { get; set; }
    [Required]
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public ICollection<BankAccountCreateDto>? BankAccounts { get; set; }
    public ICollection<ContactPersonCreateDto>? ContactPersons { get; set; }
}

public class VendorUpdateDto : VendorCreateDto
{
    public int Id { get; set; }
}

public class VendorReadDto : VendorUpdateDto
{
    public class WithChildren : VendorReadDto
    {
        public new ICollection<BankAccountReadDto>? BankAccounts { get; set; }
        public new ICollection<ContactPersonReadDto>? ContactPersons { get; set; }
    }
}

