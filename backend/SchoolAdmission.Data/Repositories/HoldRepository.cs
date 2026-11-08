using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Data.Entities;

namespace SchoolAdmission.Data.Repositories;

public class HoldRepository(AdmissionDbContext db) : IHoldRepository
{
    public void Add(Hold hold) => db.Holds.Add(hold);

    public Task<Hold?> GetByIdAsync(Guid id, CancellationToken ct) =>
        db.Holds.AsNoTracking()
            .Include(h => h.Class)
            .FirstOrDefaultAsync(h => h.Id == id, ct);

    public async Task<IReadOnlyList<Hold>> ListByStudentAsync(string studentId, CancellationToken ct) =>
        await db.Holds.AsNoTracking()
            .Include(h => h.Class)
            .Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(ct);

    public Task<Guid?> FindActiveHoldIdAsync(Guid classId, string studentId, DateTime now, CancellationToken ct) =>
        db.Holds.AsNoTracking()
            .Where(h => h.ClassId == classId && h.StudentId == studentId &&
                        (h.Status == HoldStatus.CONFIRMED ||
                         (h.Status == HoldStatus.HELD && h.ExpiresAt > now)))
            .Select(h => (Guid?)h.Id)
            .FirstOrDefaultAsync(ct);

    public Task<int> CountActiveAsync(Guid classId, DateTime now, CancellationToken ct) =>
        db.Holds.CountAsync(h => h.ClassId == classId &&
                                 (h.Status == HoldStatus.CONFIRMED ||
                                  (h.Status == HoldStatus.HELD && h.ExpiresAt > now)), ct);

    public Task<int> ExpireStaleForClassAsync(Guid classId, DateTime now, CancellationToken ct) =>
        db.Holds
            .Where(h => h.ClassId == classId && h.Status == HoldStatus.HELD && h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.EXPIRED), ct);

    public Task<int> ExpireAllStaleAsync(DateTime now, CancellationToken ct) =>
        db.Holds
            .Where(h => h.Status == HoldStatus.HELD && h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s => s.SetProperty(h => h.Status, HoldStatus.EXPIRED), ct);

    public async Task<bool> TryConfirmAsync(Guid holdId, string studentId, DateTime now, CancellationToken ct)
    {
        var changed = await db.Holds
            .Where(h => h.Id == holdId && h.StudentId == studentId &&
                        h.Status == HoldStatus.HELD && h.ExpiresAt > now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(h => h.Status, HoldStatus.CONFIRMED)
                .SetProperty(h => h.ConfirmedAt, now), ct);
        return changed == 1;
    }

    public async Task<bool> TryCancelAsync(Guid holdId, string studentId, DateTime now, CancellationToken ct)
    {
        var changed = await db.Holds
            .Where(h => h.Id == holdId && h.StudentId == studentId &&
                        h.Status == HoldStatus.HELD && h.ExpiresAt > now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(h => h.Status, HoldStatus.CANCELLED)
                .SetProperty(h => h.CancelledAt, now), ct);
        return changed == 1;
    }
}