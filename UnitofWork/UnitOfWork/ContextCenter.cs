using Microsoft.EntityFrameworkCore;
using UnitofWork.DbContexts;
using UnitofWork.InUnitOfWork;

namespace UnitofWork.UnitOfWork;

public class ContextCenter : IContextCenter
{
    private readonly TestContext _testContext;
    private readonly QuartzContext _quartzContext;
    private readonly IReadOnlyList<DbContext> _contexts;

    public TestContext TestContext => _testContext;
    public QuartzContext QuartzContext => _quartzContext;

    /// <summary>
    /// 返回所有 DbContext 的只读视图，防止外部修改内部状态。
    /// </summary>
    public IEnumerable<DbContext> GetAllContexts() => _contexts;

    public ContextCenter(TestContext testContext, QuartzContext quartzContext)
    {
        _testContext = testContext;
        _quartzContext = quartzContext;
        _contexts = new List<DbContext> { testContext, quartzContext }.AsReadOnly();
    }
}
