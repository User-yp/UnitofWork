using UnitofWork.DbContexts;
using UnitofWork.Entity;
using UnitofWork.InUnitOfWork;
using UnitofWork.IRepository;

namespace UnitofWork.Repository;

public class OrderRepository : IOrderRepository
{
    private readonly TestContext _testContext;

    public OrderRepository(IContextCenter contextCenter)
    {
        _testContext = contextCenter.TestContext;
    }

    public async Task AddOrder(Order order)
    {
        await _testContext.Orders.AddAsync(order);
    }
}
