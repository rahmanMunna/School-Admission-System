using Microsoft.EntityFrameworkCore.Storage;

namespace SchoolAdmission.Data.Repositories;

// All repositories in one request share the same scoped AdmissionDbContext,
// so a transaction started here covers every repository call in between.
public class UnitOfWork(AdmissionDbContext db) : IUnitOfWork
{
    public async Task<ITransactionScope> BeginTransactionAsync(CancellationToken ct) =>
        new TransactionScope(await db.Database.BeginTransactionAsync(ct));

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);

    private sealed class TransactionScope(IDbContextTransaction tx) : ITransactionScope
    {
        public Task CommitAsync(CancellationToken ct) => tx.CommitAsync(ct);
        public ValueTask DisposeAsync() => tx.DisposeAsync();   // rolls back if not committed
    }
}