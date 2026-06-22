using Microsoft.EntityFrameworkCore;

namespace UnitofWork.Abstractions;

/// <summary>
/// 泛型事务单元接口 — 管理单个 <typeparamref name="TContext"/> 的事务生命周期。
/// 注入此接口来操作特定数据库。
/// </summary>
public interface IUnitOfWork<TContext> : IAsyncDisposable, IDisposable
    where TContext : DbContext
{
    /// <summary>当前事务关联的 DbContext 实例。</summary>
    TContext Context { get; }

    /// <summary>开启数据库事务。</summary>
    Task BeginAsync(CancellationToken cancellationToken = default);

    /// <summary>保存所有变更并提交事务。失败时自动回滚。</summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>回滚事务，撤销所有未提交的变更。</summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);

    /// <summary>仅保存变更（不提交事务），返回受影响的行数。</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
