using InventoryReservation.Application;
using InventoryReservation.Domain;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace InventoryReservation.Api.IntegrationTests;

public sealed class FactoryRegistrationTests
{
    [Fact]
    public void Container_resolves_both_factories_and_executor_uses_each()
    {
        using var factory = new SqliteApiFactory();
        using var scope = factory.Services.CreateScope();
        var factories = scope.ServiceProvider.GetServices<IInventoryOperationFactory>().ToArray();
        Assert.Equal(2, factories.Length);
        Assert.Contains(factories, operation => operation is InboundOperationFactory);
        Assert.Contains(factories, operation => operation is OutboundOperationFactory);

        var executor = scope.ServiceProvider.GetRequiredService<InventoryOperationExecutor>();
        var product = new Product("TEST", "Test", "Tests", Guid.NewGuid());
        var warehouse = new Warehouse("Main", "Address", 1);
        var stock = warehouse.GetOrCreateStock(product);
        executor.Execute(MovementType.Receipt, stock, 3, Guid.Empty);
        Assert.Equal(3, stock.OnHandQuantity);

        var order = new SalesOrder(Guid.NewGuid());
        order.AddItem(product.Id, 3);
        order.Confirm();
        order.ReserveItem(order.Items.Single(), stock);
        order.MarkReserved();
        executor.Ship(order.Reservations.Single(), Guid.Empty);
        order.Ship();

        Assert.Equal(0, stock.OnHandQuantity);
        Assert.Equal(3, stock.Movements.Count);
        Assert.Equal(OrderStatus.Shipped, order.Status);
        Assert.Empty(typeof(Product).GetProperties()
            .Where(property => property.Name == "StockItems"));
    }

    [Fact]
    public void Invalid_shipment_metadata_does_not_modify_stock()
    {
        var executor = new InventoryOperationExecutor(
            [new InboundOperationFactory(), new OutboundOperationFactory()]);
        var product = new Product("TEST", "Test", "Tests", Guid.NewGuid());
        var stock = new Warehouse("Main", "Address", 1).GetOrCreateStock(product);
        stock.Receive(1);
        stock.Reserve(1);

        Assert.Throws<DomainRuleViolationException>(() =>
            executor.Execute(MovementType.Shipment, stock, 1, Guid.Empty));

        Assert.Equal(1, stock.OnHandQuantity);
        Assert.Equal(1, stock.ReservedQuantity);
        Assert.Empty(stock.Movements);
    }
}

