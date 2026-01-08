using Arch.Hexa.ModuMono.BuildingBlocks.Contracts.Enums;
using Arch.Hexa.ModuMono.Modules.Customers.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Arch.Hexa.ModuMono.Modules.Customers.Infrastructure.Persistence
{
    public sealed class CustomerDbContext(DbContextOptions<CustomerDbContext> options) : DbContext(options)
    {
        public DbSet<Customer> Customers => Set<Customer>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.Name).IsRequired().HasMaxLength(75);
                b.Property(x => x.Email).IsRequired().HasMaxLength(100);

                // Store the enum as an int (EF Core does this by default, but we keep it explicit)
                b.Property(x => x.Status)
                    .IsRequired()
                    //.HasConversion<int>();
                    .HasConversion(new EnumToStringConverter<CustomerStatus>())
                    .HasMaxLength(32);

                b.Property(x => x.CreatedAt).IsRequired();

                b.HasIndex(x => x.Name).HasDatabaseName("IX_Customers_Name");
                b.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UX_Customers_Email");
                b.HasIndex(x => x.Status).HasDatabaseName("IX_Customers_Status");
                b.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_Customers_CreatedAt");
                b.HasIndex(x => new { x.Status, x.CreatedAt }).HasDatabaseName("IX_Customers_Status_CreatedAt");
            });
        }
    }
}
