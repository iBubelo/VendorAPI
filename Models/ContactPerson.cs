using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VendorAPI.Models;

public class ContactPerson
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? Mail { get; set; }
    [ForeignKey("Vendor")]
    public int VendorId { get; set; }
    public Vendor? Vendor { get; set; }
}
