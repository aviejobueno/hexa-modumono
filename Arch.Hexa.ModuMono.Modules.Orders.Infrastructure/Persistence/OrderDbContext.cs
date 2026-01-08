using Arch.Hexa.ModuMono.Modules.Orders.Domain;
using Microsoft.EntityFrameworkCore;

namespace Arch.Hexa.ModuMono.Modules.Orders.Infrastructure.Persistence
{
    public sealed class OrderDbContext(DbContextOptions<OrderDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderLine> OrderLines => Set<OrderLine>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.CustomerId).IsRequired();
                b.Property(x => x.CreatedAt).IsRequired();

                b.Ignore(x => x.Total);


                // Configure relationship using backing field collection
                b.HasMany(o => o.OrderLines)
                    .WithOne(ol => ol.Order)
                    .HasForeignKey(ol => ol.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Use field access for the collection navigation
                b.Navigation(o => o.OrderLines)
                    .HasField("_lines")
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

                //b.HasOne<Customer>()
                //    .WithMany()
                //    .HasForeignKey(x => x.CustomerId)
                //    .OnDelete(DeleteBehavior.Restrict);

                b.HasIndex(x => x.CustomerId).HasDatabaseName("IX_Orders_CustomerId");
                b.HasIndex(x => x.CreatedAt).HasDatabaseName("IX_Orders_CreatedAt");
            });

            modelBuilder.Entity<OrderLine>(b =>
            {
                b.HasKey(x => x.Id);

                b.Property(x => x.OrderId).IsRequired();
                b.Property(x => x.ProductName).IsRequired().HasMaxLength(300);
                b.Property(x => x.Quantity).IsRequired();
                b.Property(x => x.UnitPrice).IsRequired();

                b.HasIndex(x => x.OrderId).HasDatabaseName("IX_OrderLines_OrderId");
            });

        }
    }
}
