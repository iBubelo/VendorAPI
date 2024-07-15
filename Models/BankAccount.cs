using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorAPI.Models;

public class BankAccount
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? IBAN { get; set; }
    public string? BIC { get; set; }
    public string? Name { get; set; }
    [ForeignKey("Vendor")]
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }
}
