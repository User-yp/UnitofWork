using UnitofWork.IRepository;

namespace UnitofWork.InUnitOfWork;

public interface IRepoFactory
{
    IUnitOfWork UnitOfWork { get; }
    IOrderRepository OrderRepository { get; }
    IQuartzRepository QuartzRepository { get; }
}
