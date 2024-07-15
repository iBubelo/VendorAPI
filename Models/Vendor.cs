using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorAPI.Models;

public class Vendor
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Name2 { get; set; }
    public string? Address1 { get; set; }
    public string? Address2 { get; set; }
    public string? ZIP { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Mail { get; set; }
    public string? Phone { get; set; }
    public string? Notes { get; set; }
    public ICollection<BankAccount>? BankAccounts { get; set; }
    public ICollection<ContactPerson>? ContactPersons { get; set; }
}
