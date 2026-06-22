using UnitofWork.InUnitOfWork;
using UnitofWork.IRepository;
using UnitofWork.Repository;

namespace UnitofWork.UnitOfWork;

public class RepoFactory : IRepoFactory
{
    public IUnitOfWork UnitOfWork { get; }
    public IOrderRepository OrderRepository { get; }
    public IQuartzRepository QuartzRepository { get; }

    public RepoFactory(IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
        // 缓存 Repository 实例，避免每次访问属性都创建新对象
        OrderRepository = new OrderRepository(unitOfWork.ContextCenter);
        QuartzRepository = new QuartzRepository(unitOfWork.ContextCenter);
        // 注意：不再在构造函数中调用 BeginAsync — 由调用方显式管理事务边界
    }
}
