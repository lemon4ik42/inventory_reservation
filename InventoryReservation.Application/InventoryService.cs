using InventoryReservation.Domain;

namespace InventoryReservation.Application;

public sealed class InventoryService
{
    public InventoryService(
        ISalesOrderRepository orders,
        IStockRepository stock,
        IInventoryUnitOfWork unitOfWork,
        IWarehouseSelectionStrategy warehouseSelection,
        InventoryOperationExecutor operations)
    {
        _orders = orders;
        _stock = stock;
        _unitOfWork = unitOfWork;
        _warehouseSelection = warehouseSelection;
        _operations = operations;
    }

    private readonly ISalesOrderRepository _orders;
    private readonly IStockRepository _stock;
    private readonly IInventoryUnitOfWork _unitOfWork;
    private readonly IWarehouseSelectionStrategy _warehouseSelection;
    private readonly InventoryOperationExecutor _operations;

    public async Task ReceiveAsync(
        Guid warehouseId,
        Guid productId,
        int quantity,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var warehouse = await _stock.GetWarehouseAsync(warehouseId, cancellationToken)
            ?? throw new KeyNotFoundException("Warehouse not found.");
        var product = await _stock.GetProductAsync(productId, cancellationToken)
            ?? throw new KeyNotFoundException("Product not found.");
        var stockItem = warehouse.GetOrCreateStock(product);
        _operations.Execute(MovementType.Receipt, stockItem, quantity, userId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ReserveAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await GetOrderAsync(orderId, cancellationToken);
        order.EnsureStatus(OrderStatus.Confirmed);
        foreach (var line in order.Items)
        {
            var candidates = await _stock.GetAvailableForProductAsync(line.ProductId, cancellationToken);
            var eligible = candidates.Where(item => item.AvailableQuantity >= line.Quantity).ToArray();
            var stockItem = _warehouseSelection.Select(eligible);
            order.ReserveItem(line, stockItem);
        }

        order.MarkReserved();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await GetOrderAsync(orderId, cancellationToken);
        order.Cancel();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ShipAsync(Guid orderId, Guid userId, CancellationToken cancellationToken)
    {
        var order = await GetOrderAsync(orderId, cancellationToken);
        order.EnsureStatus(OrderStatus.Reserved);
        foreach (var reservation in order.Reservations)
        {
            _operations.Ship(reservation, userId);
        }

        order.Ship();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<SalesOrder> CreateOrderAsync(Guid customerId, CancellationToken cancellationToken)
    {
        var order = new SalesOrder(customerId);
        await _orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return order;
    }

    public async Task AddOrderItemAsync(
        Guid orderId,
        Guid productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        var product = await _stock.GetProductAsync(productId, cancellationToken)
            ?? throw new KeyNotFoundException("Product not found.");
        product.EnsureActive();
        var order = await GetOrderAsync(orderId, cancellationToken);
        order.AddItem(productId, quantity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ConfirmAsync(Guid orderId, CancellationToken cancellationToken)
    {
        var order = await GetOrderAsync(orderId, cancellationToken);
        order.Confirm();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<SalesOrder> GetOrderAsync(Guid orderId, CancellationToken cancellationToken)
    {
        return await _orders.GetForUpdateAsync(orderId, cancellationToken)
            ?? throw new KeyNotFoundException("Order not found.");
    }
}

