namespace VendorAPI.DTOs;

public class ContactPersonCreateDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public required string Phone { get; set; }
    public string? Mail { get; set; }
    public int VendorId { get; set; }
}

public class ContactPersonUpdateDto : ContactPersonCreateDto
{
    public int Id { get; set; }
}

public class ContactPersonReadDto : ContactPersonUpdateDto
{
    public class CPWithVendor : ContactPersonReadDto
    {
        public VendorReadDto? Vendor { get; set; }
    }
}
