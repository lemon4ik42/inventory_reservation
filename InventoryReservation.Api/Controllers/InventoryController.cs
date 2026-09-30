using InventoryReservation.Application;
using InventoryReservation.Domain;
using Microsoft.AspNetCore.Mvc;

namespace InventoryReservation.Api.Controllers;

[ApiController]
[Route("api")]
public sealed class InventoryController : ControllerBase
{
    public InventoryController(CatalogService catalog, InventoryService inventoryService)
    {
        _catalog = catalog;
        _inventoryService = inventoryService;
    }

    private readonly CatalogService _catalog;
    private readonly InventoryService _inventoryService;

    [HttpPost("products")]
    public async Task<ActionResult<Product>> CreateProduct(Product product,
        CancellationToken cancellationToken)
    {
        await _catalog.CreateProductAsync(product, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpGet("products/{id:guid}")]
    public async Task<ActionResult<Product>> GetProduct(Guid id, CancellationToken cancellationToken)
    {
        var product = await _catalog.GetProductAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("products")]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts(
        int page = 1,
        int pageSize = 20,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var products = await _catalog.GetProductsAsync(page, pageSize, category, cancellationToken);
        return Ok(products);
    }

    [HttpPost("warehouses")]
    public async Task<ActionResult<Warehouse>> CreateWarehouse(Warehouse warehouse,
        CancellationToken cancellationToken)
    {
        await _catalog.CreateWarehouseAsync(warehouse, cancellationToken);
        return Created($"/api/warehouses/{warehouse.Id}", warehouse);
    }

    [HttpGet("stock")]
    public async Task<ActionResult<IEnumerable<StockItem>>> GetStock(
        Guid? warehouseId,
        Guid? productId,
        int? minAvailable,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var stockItems = await _catalog.GetStockAsync(warehouseId,
            productId,
            minAvailable,
            page,
            pageSize,
            cancellationToken);
        return Ok(stockItems);
    }

    [HttpPost("stock/receipts")]
    public async Task<IActionResult> Receive(
        Guid warehouseId,
        Guid productId,
        int quantity,
        Guid userId,
        CancellationToken cancellationToken)
    {
        await _inventoryService.ReceiveAsync(warehouseId, productId, quantity, userId, cancellationToken);
        return NoContent();
    }

    [HttpPost("orders")]
    public async Task<ActionResult<SalesOrder>> CreateOrder(Guid customerId,
        CancellationToken cancellationToken)
    {
        var order = await _inventoryService.CreateOrderAsync(customerId, cancellationToken);
        return Created($"/api/orders/{order.Id}", order);
    }

    [HttpPost("orders/{id:guid}/items")]
    public async Task<IActionResult> AddOrderItem(
        Guid id,
        Guid productId,
        int quantity,
        CancellationToken cancellationToken)
    {
        await _inventoryService.AddOrderItemAsync(id, productId, quantity, cancellationToken);
        return NoContent();
    }

    [HttpPost("orders/{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        await _inventoryService.ConfirmAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("orders/{id:guid}/reserve")]
    public async Task<IActionResult> Reserve(Guid id, CancellationToken cancellationToken)
    {
        await _inventoryService.ReserveAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("orders/{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        await _inventoryService.CancelAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("orders/{id:guid}/ship")]
    public async Task<IActionResult> Ship(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        await _inventoryService.ShipAsync(id, userId, cancellationToken);
        return NoContent();
    }

    [HttpGet("orders/{id:guid}")]
    public async Task<ActionResult<SalesOrder>> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await _inventoryService.GetOrderAsync(id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpGet("inventory-movements")]
    public async Task<ActionResult<IEnumerable<InventoryMovement>>> GetMovements(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var movements = await _catalog.GetMovementsAsync(page, pageSize, cancellationToken);
        return Ok(movements);
    }
}

