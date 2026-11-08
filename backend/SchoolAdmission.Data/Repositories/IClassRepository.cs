using SchoolAdmission.Data.Entities;

namespace SchoolAdmission.Data.Repositories;

public interface IClassRepository
{
    void Add(SchoolClass schoolClass);

    // SELECT … FOR UPDATE: locks this class row until the transaction ends.
    // MUST be called inside a transaction (IUnitOfWork.BeginTransactionAsync).
    Task<SchoolClass?> GetForUpdateAsync(Guid id, CancellationToken ct);

    // Seat counts treat a HELD row as expired the moment expires_at <= now.
    Task<IReadOnlyList<ClassSeatCounts>> ListWithSeatCountsAsync(DateTime now, CancellationToken ct);
    Task<ClassSeatCounts?> GetWithSeatCountsAsync(Guid id, DateTime now, CancellationToken ct);
}