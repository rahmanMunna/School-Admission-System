namespace SchoolAdmission.Data.Repositories;

// Lets the Business tier run several repository calls in ONE database transaction
// (needed for: lock class row → count → insert → commit).
public interface IUnitOfWork
{
    Task<ITransactionScope> BeginTransactionAsync(CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

// Dispose WITHOUT Commit = rollback. Always use with "await using".
public interface ITransactionScope : IAsyncDisposable
{
    Task CommitAsync(CancellationToken ct);
}