using UnitofWork.Entity;

namespace UnitofWork.IRepository;

public interface IOrderRepository
{
    Task AddOrder(Order order);
}
