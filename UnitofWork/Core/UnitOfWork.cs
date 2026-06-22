using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using UnitofWork.Abstractions;

namespace UnitofWork.Core;

/// <summary>
/// 内部非泛型事务单元接口，供 <see cref="UnitOfWorkManager"/> 统一调度。
/// </summary>
internal interface IUnitOfWorkInternal
{
    DbContext Context { get; }
    Task BeginAsync(CancellationToken cancellationToken);
    Task<int> CommitAsync(CancellationToken cancellationToken);
    Task RollbackAsync(CancellationToken cancellationToken);
}

/// <summary>
/// 泛型事务单元实现 — 管理单个 DbContext 的事务。
/// </summary>
public class UnitOfWork<TContext> : IUnitOfWork<TContext>, IUnitOfWorkInternal
    where TContext : DbContext
{
    private readonly TContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed;

    public TContext Context => _context;
    DbContext IUnitOfWorkInternal.Context => _context;

    public UnitOfWork(TContext context)
    {
        _context = context;
    }

    public async Task BeginAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (_transaction is not null)
            return;

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        try
        {
            var count = await SaveChangesAsync(cancellationToken);

            if (_transaction is not null)
            {
                await _transaction.CommitAsync(cancellationToken);
            }
            return count;
        }
        catch
        {
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        await DisposeTransactionAsync();
    }

    private async Task DisposeTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        await DisposeTransactionAsync();
        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _transaction?.Dispose();
        _transaction = null;
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(typeof(UnitOfWork<TContext>).Name);
    }
}
