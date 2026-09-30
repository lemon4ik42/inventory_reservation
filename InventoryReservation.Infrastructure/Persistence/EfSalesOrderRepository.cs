using InventoryReservation.Domain;
using Microsoft.EntityFrameworkCore;

namespace InventoryReservation.Infrastructure;

public sealed class EfSalesOrderRepository : ISalesOrderRepository
{
    public EfSalesOrderRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private readonly InventoryDbContext _dbContext;

    public Task AddAsync(SalesOrder order, CancellationToken cancellationToken)
    {
        _dbContext.SalesOrders.Add(order);
        return Task.CompletedTask;
    }

    public Task<SalesOrder?> GetForUpdateAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.SalesOrders.Include(order => order.Items).Include(order => order.Reservations)
            .ThenInclude(reservation => reservation.StockItem)
            .ThenInclude(item => item.Product)
            .SingleOrDefaultAsync(order => order.Id == id, cancellationToken);
    }
}

