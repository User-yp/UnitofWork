using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using UnitofWork.Abstractions;

namespace UnitofWork.Core;

/// <summary>
/// 多上下文事务协调器 — 协调多个 <see cref="IUnitOfWork{TContext}"/> 的事务。
/// 通过 DI 的 <see cref="IServiceProvider"/> 动态解析 DbContext。
/// </summary>
public class UnitOfWorkManager : IUnitOfWorkManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, IUnitOfWorkInternal> _uowCache = new();
    private bool _disposed;

    public UnitOfWorkManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    public IUnitOfWork<TContext> Get<TContext>() where TContext : DbContext
    {
        ThrowIfDisposed();

        var type = typeof(TContext);
        if (_uowCache.TryGetValue(type, out var cached))
            return (IUnitOfWork<TContext>)cached;

        var context = _serviceProvider.GetRequiredService<TContext>();
        var uow = new UnitOfWork<TContext>(context);
        _uowCache[type] = uow;
        return uow;
    }

    /// <inheritdoc />
    /// <remarks>
    /// 注意：此方法只能为已通过 <see cref="Get{TContext}"/> 初始化的上下文开启事务。
    /// 建议在调用此方法前，先通过 Get&lt;TContext&gt;() 获取所有需要的 UoW 实例。
    /// </remarks>
    public async Task BeginAllAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (_uowCache.Count == 0)
            throw new InvalidOperationException(
                "No UnitOfWork instances have been created. Call Get<TContext>() first for each DbContext.");

        foreach (var uow in _uowCache.Values)
        {
            await uow.BeginAsync(cancellationToken);
        }
    }

    /// <inheritdoc />
    public async Task<int> CommitAllAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (_uowCache.Count == 0) return 0;

        int total = 0;
        try
        {
            foreach (var uow in _uowCache.Values)
            {
                total += await uow.CommitAsync(cancellationToken);
            }
            return total;
        }
        catch
        {
            await RollbackAllAsync(cancellationToken);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task RollbackAllAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        foreach (var uow in _uowCache.Values)
        {
            await uow.RollbackAsync(cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var uow in _uowCache.Values)
        {
            await ((IAsyncDisposable)uow).DisposeAsync();
        }
        _uowCache.Clear();

        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        foreach (var uow in _uowCache.Values)
        {
            ((IDisposable)uow).Dispose();
        }
        _uowCache.Clear();

        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(UnitOfWorkManager));
    }
}
