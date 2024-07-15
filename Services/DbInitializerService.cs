using Microsoft.AspNetCore.Identity;
using VendorAPI.Models;

namespace VendorAPI.Services;

public class DbInitializerService
{
    private readonly ApplicationDbContext _context;

    public DbInitializerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Initialize()
    {
        _context.Database.EnsureCreated();
        SeedData();
    }

    private void SeedData()
    {
        if (!_context.Vendors.Any())
        {
            _context.Vendors.AddRange(
                new Vendor
                {
                    Name = "Acme Corporation",
                    Name2 = "Acme Corp",
                    Address1 = "123 Main Street",
                    Address2 = "Suite 456",
                    ZIP = "12345",
                    Country = "United States",
                    City = "Anytown",
                    Mail = "info@acmecorp.com",
                    Phone = "+1 (555) 123-4567",
                    Notes = "Preferred supplier for widgets",
                },
                new Vendor
                {
                    Name = "Widget World",
                    Name2 = "Widget World Inc",
                    Address1 = "456 Elm Street",
                    Address2 = "Unit 789",
                    ZIP = "54321",
                    Country = "United States",
                    City = "Springfield",
                    Mail = "info@widgets.com",
                    Phone = "+1 (555) 987-6543",
                    Notes = "Specializes in custom widgets",
                },
                new Vendor
                {
                    Name = "Gadget Galaxy",
                    Name2 = "Gadget Galaxy LLC",
                    Address1 = "789 Oak Street",
                    Address2 = "Apt 123",
                    ZIP = "67890",
                    Country = "United States",
                    City = "Metroville",
                    Mail = "info@galaxy.com",
                    Phone = "+1 (555) 456-7890",
                    Notes = "Innovative gadgets for all ages",
                    BankAccounts = new List<BankAccount>
                    {
                        new BankAccount
                        {
                            IBAN = "DE75 3704 0044 0532 0130 00",
                            BIC = "COBADEFFXXX",
                            Name = "TS Hauptkonto"
                        }
                    },
                    ContactPersons = new List<ContactPerson>
                    {
                        new ContactPerson
                        {
                            FirstName = "Hans",
                            LastName = "Mueller",
                            Phone = "+49 30 2345678",
                            Mail = "hans.mueller@techsolutions.de"
                        },
                        new ContactPerson
                        {
                            FirstName = "Lena",
                            LastName = "Schmidt",
                            Phone = "+49 30 3456789",
                            Mail = "lena.schmidt@techsolutions.de"
                        }
                    }
                }
            );
            _context.SaveChanges();
        }

        var hasher = new PasswordHasher<User>();
        if (!_context.Users.Any())
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _context.Users.AddRange(
            new User
            {
                Id = "1",
                UserName = "admin@example.com",
                NormalizedUserName = "ADMIN@EXAMPLE.COM",
                Email = "admin@example.com",
                NormalizedEmail = "ADMIN@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Admin123!"),
                SecurityStamp = string.Empty
            },
            new User
            {
                Id = "2",
                UserName = "manager@example.com",
                NormalizedUserName = "MANAGER@EXAMPLE.COM",
                Email = "manager@example.com",
                NormalizedEmail = "MANAGER@EXAMPLE.COM",
                EmailConfirmed = true,
                PasswordHash = hasher.HashPassword(null, "Manager123!"),
                SecurityStamp = string.Empty
            });
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            _context.SaveChanges();
        }

        // Seed IdentityRole
        if (!_context.Roles.Any())
        {
            _context.Roles.AddRange(
                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "Manager",
                    NormalizedName = "MANAGER"
                }
            );
            _context.SaveChanges();
        }

        // Assign admin role to test user
        if (!_context.UserRoles.Any())
        {
            _context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    UserId = "1",
                    RoleId = "1"
                },
            new IdentityUserRole<string>
            {
                UserId = "2",
                RoleId = "2"
            });
            _context.SaveChanges();
        }
    }
}