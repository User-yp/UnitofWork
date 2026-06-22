using UnitofWork.Abstractions;
using UnitofWork.WebApi.Data;
using UnitofWork.WebApi.Data.Entities;

namespace UnitofWork.WebApi.Domain;

public class DomainService
{
    private readonly IUnitOfWorkManager _manager;

    public DomainService(IUnitOfWorkManager manager)
    {
        _manager = manager;
    }

    public async Task<bool> SaveAsync()
    {
        try
        {
            // 获取各 DbContext 的事务单元
            var testUow = _manager.Get<TestContext>();
            var quartzUow = _manager.Get<QuartzContext>();

            // 统一开启事务
            await _manager.BeginAllAsync();

            // 业务操作
            testUow.Context.Orders.Add(new Order("Test", "test"));
            quartzUow.Context.JobConfigs.Add(
                new JobConfig("Group1", "TestJob", null, "testname", null, "aas", false, null));

            // 统一提交（内部含失败回滚）
            await _manager.CommitAllAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"保存失败: {ex.Message}");
            return false;
        }
    }
}
