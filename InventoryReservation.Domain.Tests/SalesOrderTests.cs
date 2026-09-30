using Xunit;

namespace InventoryReservation.Domain.Tests;

public sealed class SalesOrderTests
{
    [Fact]
    public void Complete_lifecycle_consumes_reserved_stock()
    {
        var product = new Product("TEST", "Test", "Tests", Guid.NewGuid());
        var stock = new Warehouse("Main", "Address", 1).GetOrCreateStock(product);
        stock.Receive(2);
        var order = new SalesOrder(Guid.NewGuid());
        order.AddItem(product.Id, 2);
        order.Confirm();
        order.ReserveItem(order.Items.Single(), stock);
        order.MarkReserved();
        order.Reservations.Single().Ship(new TestShipmentOperation());
        order.Ship();
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Equal(0, stock.OnHandQuantity);
    }

    [Fact]
    public void Draft_order_cannot_be_shipped()
    {
        var order = new SalesOrder(Guid.NewGuid());
        Assert.Throws<DomainRuleViolationException>(order.Ship);
    }

    [Fact]
    public void Cancellation_releases_fully_reserved_stock()
    {
        var product = new Product("TEST", "Test", "Tests", Guid.NewGuid());
        var stock = new Warehouse("Main", "Address", 1).GetOrCreateStock(product);
        stock.Receive(2);
        var order = new SalesOrder(Guid.NewGuid());
        order.AddItem(product.Id, 2);
        order.Confirm();
        order.ReserveItem(order.Items.Single(), stock);
        order.MarkReserved();
        order.Cancel();
        Assert.Equal(2, stock.AvailableQuantity);
        Assert.False(order.Reservations.Single().IsActive);
        Assert.Throws<DomainRuleViolationException>(order.Cancel);
    }

    [Fact]
    public void Incomplete_reservation_cannot_change_order_status()
    {
        var order = new SalesOrder(Guid.NewGuid());
        order.AddItem(Guid.NewGuid(), 1);
        order.Confirm();
        Assert.Throws<DomainRuleViolationException>(order.MarkReserved);
    }

    [Fact]
    public void Warehouse_reuses_stock_for_same_product()
    {
        var warehouse = new Warehouse("Main", "Address", 1);
        var product = new Product("TEST", "Test", "Tests", Guid.NewGuid());
        Assert.Same(warehouse.GetOrCreateStock(product), warehouse.GetOrCreateStock(product));
        Assert.Single(warehouse.StockItems);
    }

    [Fact]
    public void Shipment_cannot_complete_without_consuming_stock()
    {
        var product = new Product("TEST", "Test", "Tests", Guid.NewGuid());
        var stock = new Warehouse("Main", "Address", 1).GetOrCreateStock(product);
        stock.Receive(2);
        var order = new SalesOrder(Guid.NewGuid());
        order.AddItem(product.Id, 2);
        order.Confirm();
        order.ReserveItem(order.Items.Single(), stock);
        var reservation = order.Reservations.Single();

        Assert.Throws<DomainRuleViolationException>(() => reservation.Ship(new NoOpStockOperation()));
        Assert.True(reservation.IsActive);
        Assert.Equal(2, stock.ReservedQuantity);
    }
}

