using SchoolAdmission.Data.Entities;

namespace SchoolAdmission.Data.Repositories;

public interface IHoldRepository
{
    void Add(Hold hold);

    // Includes the Class (for ClassName). Read-only.
    Task<Hold?> GetByIdAsync(Guid id, CancellationToken ct);

    // Newest first, with Class included.
    Task<IReadOnlyList<Hold>> ListByStudentAsync(string studentId, CancellationToken ct);

    // "Active" = CONFIRMED, or HELD with expires_at > now.
    Task<Guid?> FindActiveHoldIdAsync(Guid classId, string studentId, DateTime now, CancellationToken ct);
    Task<int> CountActiveAsync(Guid classId, DateTime now, CancellationToken ct);

    // Marks time-expired HELD rows as EXPIRED. Returns how many rows changed.
    Task<int> ExpireStaleForClassAsync(Guid classId, DateTime now, CancellationToken ct);
    Task<int> ExpireAllStaleAsync(DateTime now, CancellationToken ct);   // background sweeper

    // Conditional updates: true only if THIS call changed the row (HELD + owner + not expired).
    Task<bool> TryConfirmAsync(Guid holdId, string studentId, DateTime now, CancellationToken ct);
    Task<bool> TryCancelAsync(Guid holdId, string studentId, DateTime now, CancellationToken ct);
}