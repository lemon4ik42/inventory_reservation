using Xunit;

namespace InventoryReservation.Domain.Tests;

public sealed class StockItemTests
{
    [Fact]
    public void Reserve_reduces_available_quantity()
    {
        var item = new StockItem(
            new Product("TEST", "Test", "Tests", Guid.NewGuid()),
            new Warehouse("Main", "Address", 1));
        item.Receive(10);
        item.Reserve(4);
        Assert.Equal(6, item.AvailableQuantity);
        Assert.Equal(4, item.ReservedQuantity);
    }

    [Fact]
    public void Reserve_more_than_available_is_rejected()
    {
        var item = new StockItem(
            new Product("TEST", "Test", "Tests", Guid.NewGuid()),
            new Warehouse("Main", "Address", 1));
        item.Receive(1);
        Assert.Throws<DomainRuleViolationException>(() => item.Reserve(2));
    }

    [Fact]
    public void Release_reservation_makes_stock_available()
    {
        var item = new StockItem(
            new Product("TEST", "Test", "Tests", Guid.NewGuid()),
            new Warehouse("Main", "Address", 1));
        item.Receive(5);
        item.Reserve(3);
        item.ReleaseReservation(2);
        Assert.Equal(4, item.AvailableQuantity);
    }

    [Fact]
    public void Shipping_reduces_on_hand_and_reservation()
    {
        var item = new StockItem(
            new Product("TEST", "Test", "Tests", Guid.NewGuid()),
            new Warehouse("Main", "Address", 1));
        item.Receive(5);
        item.Reserve(3);
        item.ShipReserved(3);
        Assert.Equal(2, item.OnHandQuantity);
        Assert.Equal(0, item.ReservedQuantity);
    }

    [Fact]
    public void Quantity_overflow_is_rejected_without_changing_stock()
    {
        var product = new Product("TEST", "Test", "Tests", Guid.NewGuid());
        var stock = new Warehouse("Main", "Address", 1).GetOrCreateStock(product);
        stock.Receive(int.MaxValue);

        Assert.Throws<DomainRuleViolationException>(() => stock.Receive(1));
        Assert.Equal(int.MaxValue, stock.OnHandQuantity);
    }
}

