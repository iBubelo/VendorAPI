using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VendorAPI.Models;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<Vendor> Vendors { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<ContactPerson> ContactPersons { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vendor>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Name2).HasMaxLength(255);
            entity.Property(e => e.Address1).HasMaxLength(255);
            entity.Property(e => e.Address2).HasMaxLength(255);
            entity.Property(e => e.ZIP).HasMaxLength(20);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.Mail).HasMaxLength(320);
            entity.Property(e => e.Phone).HasMaxLength(15);

            entity.HasMany(e => e.BankAccounts)
                .WithOne(e => e.Vendor)
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.ContactPersons)
                .WithOne(e => e.Vendor)
                .HasForeignKey(e => e.VendorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BankAccount>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.IBAN).IsRequired().HasMaxLength(34);
            entity.Property(e => e.BIC).IsRequired().HasMaxLength(11);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(255);

            entity.HasOne(e => e.Vendor)
                .WithMany(v => v.BankAccounts)
                .HasForeignKey(e => e.VendorId);
        });

        modelBuilder.Entity<ContactPerson>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.Mail).HasMaxLength(320);

            entity.HasOne(e => e.Vendor)
                .WithMany(v => v.ContactPersons)
                .HasForeignKey(e => e.VendorId);
        });
    }
}