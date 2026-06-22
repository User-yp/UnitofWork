using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UnitofWork.InUnitOfWork;

namespace UnitofWork.UnitOfWork;

public class UnitOfWork : IUnitOfWork, IAsyncDisposable
{
    private readonly IReadOnlyList<DbContext> _contexts;
    private readonly IContextCenter _contextCenter;
    private readonly Dictionary<DbContext, IDbContextTransaction> _transactions = new();
    private bool _disposed;

    public IContextCenter ContextCenter => _contextCenter;

    public UnitOfWork(IContextCenter contextCenter)
    {
        _contexts = contextCenter.GetAllContexts().ToList();
        _contextCenter = contextCenter;
    }

    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        var started = new List<DbContext>();
        try
        {
            foreach (var context in _contexts)
            {
                var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                _transactions[context] = transaction;
                started.Add(context);
            }
        }
        catch
        {
            // 回滚已成功开启的事务，避免事务泄漏
            foreach (var context in started)
            {
                if (_transactions.Remove(context, out var t))
                {
                    await t.DisposeAsync();
                }
            }
            throw;
        }
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        try
        {
            // 先保存所有变更
            var count = await SaveChangesAsync(cancellationToken);

            // 再提交所有事务
            foreach (var transaction in _transactions.Values)
            {
                await transaction.CommitAsync(cancellationToken);
            }

            // 提交成功后清理事务资源
            await DisposeTransactionsAsync();
            return count;
        }
        catch
        {
            // 保存或提交失败时回滚所有事务
            await RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        int totalChanges = 0;
        foreach (var context in _transactions.Keys)
        {
            totalChanges += await context.SaveChangesAsync(cancellationToken);
        }
        return totalChanges;
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        foreach (var context in _contexts)
        {
            if (_transactions.TryGetValue(context, out var transaction))
            {
                await transaction.RollbackAsync(cancellationToken);
            }
        }
        // 回滚后清理事务资源
        await DisposeTransactionsAsync();
    }

    private async Task DisposeTransactionsAsync()
    {
        foreach (var transaction in _transactions.Values)
        {
            await transaction.DisposeAsync();
        }
        _transactions.Clear();
    }

    /// <summary>
    /// 异步释放所有事务和 DbContext 资源。
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await DisposeTransactionsAsync();

        foreach (var context in _contexts)
        {
            await context.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 同步释放 — 使用同步 API 避免潜在的异步阻塞死锁。
    /// EF Core 的 IDbContextTransaction 和 DbContext 同时实现了 IDisposable，
    /// 因此同步路径直接调用同步 Dispose，不阻塞异步操作。
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var transaction in _transactions.Values)
        {
            transaction.Dispose();
        }
        _transactions.Clear();

        foreach (var context in _contexts)
        {
            context.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UnitOfWork));
    }
}
