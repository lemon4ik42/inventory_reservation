using InventoryReservation.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryReservation.Infrastructure;

public sealed class InventoryDbContext : DbContext, IInventoryUnitOfWork
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();

    public DbSet<StockItem> StockItems => Set<StockItem>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();

    public DbSet<SalesOrderItem> SalesOrderItems => Set<SalesOrderItem>();
    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    public DbSet<InventoryMovement> InventoryMovements => Set<InventoryMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SalesOrder>().Property(order => order.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedNever();
        modelBuilder.Entity<StockItem>().Property(item => item.Id).ValueGeneratedNever();
        modelBuilder.Entity<InventoryMovement>().Property(movement => movement.Id).ValueGeneratedNever();
        modelBuilder.Entity<StockReservation>().Property(reservation => reservation.Id).ValueGeneratedNever();
        modelBuilder.Entity<SalesOrderItem>().Property(item => item.Id).ValueGeneratedNever();
        ConfigureIndexes(modelBuilder);
        ConfigureRelationships(modelBuilder);
        ConfigureSeedData(modelBuilder);
    }

    private static void ConfigureIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasIndex(product => product.Sku).IsUnique();
        modelBuilder.Entity<StockItem>().HasIndex(item => new { item.ProductId, item.WarehouseId })
            .IsUnique();
        modelBuilder.Entity<StockItem>().Property(item => item.RowVersion)
            .IsConcurrencyToken()
            .ValueGeneratedNever();
        modelBuilder.Entity<Product>().Property(product => product.Sku).HasMaxLength(64);
        modelBuilder.Entity<Product>().Property(product => product.Name).HasMaxLength(256);
        modelBuilder.Entity<Warehouse>().Property(warehouse => warehouse.Name)
            .HasMaxLength(256);
    }

    private static void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockItem>().HasOne(item => item.Warehouse).WithMany(warehouse => warehouse.StockItems)
            .HasForeignKey(item => item.WarehouseId);
        modelBuilder.Entity<StockItem>().HasOne(item => item.Product).WithMany()
            .HasForeignKey(item => item.ProductId);
        modelBuilder.Entity<StockItem>().HasMany(item => item.Movements).WithOne()
            .HasForeignKey(movement => movement.StockItemId);
        modelBuilder.Entity<StockReservation>().HasOne(reservation => reservation.StockItem)
            .WithMany()
            .HasForeignKey(reservation => reservation.StockItemId)
            .OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<SalesOrder>().HasMany(order => order.Items).WithOne()
            .HasForeignKey(item => item.SalesOrderId);
        modelBuilder.Entity<SalesOrder>().HasMany(order => order.Reservations).WithOne()
            .HasForeignKey(reservation => reservation.SalesOrderId);
    }

    private static void ConfigureSeedData(ModelBuilder modelBuilder)
    {
        var supplierId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var customerId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var userId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        var warehouseId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var productId = Guid.Parse("55555555-5555-5555-5555-555555555555");
        modelBuilder.Entity<Supplier>().HasData(new { Id = supplierId, Name = "Demo supplier" });
        modelBuilder.Entity<Customer>().HasData(new { Id = customerId, Name = "Demo customer" });
        modelBuilder.Entity<ApplicationUser>().HasData(new { Id = userId, UserName = "warehouse.demo" });
        modelBuilder.Entity<Warehouse>().HasData(new
        {
            Id = warehouseId,
            Name = "Main warehouse",
            Address = "Demo street, 1",
            Priority = 100
        });
        modelBuilder.Entity<Product>().HasData(new
        {
            Id = productId,
            Sku = "DEMO-001",
            Name = "Demo product",
            Category = "Demo",
            SupplierId = supplierId,
            IsActive = true
        });
    }
}

