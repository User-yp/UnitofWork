namespace UnitofWork.InUnitOfWork;

public interface IUnitOfWork : IDisposable
{
    IContextCenter ContextCenter { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginAsync(CancellationToken cancellationToken = default);
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
