using Microsoft.EntityFrameworkCore.Storage;

namespace SchoolAdmission.Data.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AdmissionDbContext _db;

    // Constructor
    public UnitOfWork(AdmissionDbContext db)
    {
        _db = db;
    }

    // Begin database transaction
    public async Task<ITransactionScope> BeginTransactionAsync(
        CancellationToken ct)
    {
        var transaction = await _db.Database.BeginTransactionAsync(ct);

        return new TransactionScope(transaction);
    }

    // Save changes to the database
    public async Task SaveChangesAsync(CancellationToken ct)
    {
        await _db.SaveChangesAsync(ct);
    }

    // Inner class to manage transaction
    private sealed class TransactionScope : ITransactionScope
    {
        private readonly IDbContextTransaction _tx;

        // Constructor
        public TransactionScope(IDbContextTransaction tx)
        {
            _tx = tx;
        }

        // Commit transaction
        public async Task CommitAsync(CancellationToken ct)
        {
            await _tx.CommitAsync(ct);
        }

        // Dispose transaction
        public async ValueTask DisposeAsync()
        {
            await _tx.DisposeAsync();
        }
    }
}