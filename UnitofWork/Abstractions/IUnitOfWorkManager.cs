using Microsoft.EntityFrameworkCore;

namespace UnitofWork.Abstractions;

/// <summary>
/// 多上下文事务协调器 — 跨多个 DbContext 管理分布式事务。
/// 当需要在一次操作中协调多个数据库时注入此接口。
/// </summary>
public interface IUnitOfWorkManager : IAsyncDisposable, IDisposable
{
    /// <summary>获取指定 <typeparamref name="TContext"/> 的事务单元。</summary>
    IUnitOfWork<TContext> Get<TContext>() where TContext : DbContext;

    /// <summary>为所有已注册的 DbContext 开启事务。</summary>
    Task BeginAllAsync(CancellationToken cancellationToken = default);

    /// <summary>提交所有已注册 DbContext 的事务。任一失败则全部回滚。</summary>
    Task<int> CommitAllAsync(CancellationToken cancellationToken = default);

    /// <summary>回滚所有已注册 DbContext 的事务。</summary>
    Task RollbackAllAsync(CancellationToken cancellationToken = default);
}
