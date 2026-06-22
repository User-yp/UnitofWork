using UnitofWork.Entity;
using UnitofWork.InUnitOfWork;
using UnitofWork.IRepository;

namespace UnitofWork.Domain;

public class DomainService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IQuartzRepository _quartzRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DomainService(IRepoFactory repo)
    {
        _orderRepository = repo.OrderRepository;
        _quartzRepository = repo.QuartzRepository;
        _unitOfWork = repo.UnitOfWork;
    }

    public async Task<bool> SaveAsync()
    {
        try
        {
            // 显式开启分布式事务
            await _unitOfWork.BeginAsync();

            await _orderRepository.AddOrder(new Order("Test", "test"));
            await _quartzRepository.AddJobConfig(new JobConfig("Group1", "TestJob", null, "testname", null, "aas", false, null));

            // CommitAsync 内部已包含失败回滚逻辑
            await _unitOfWork.CommitAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保存失败: {ex.Message}");
            return false;
        }
    }
}
