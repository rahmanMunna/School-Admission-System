using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Data.Entities;

namespace SchoolAdmission.Data.Repositories;

public class HoldRepository : IHoldRepository
{
    private readonly AdmissionDbContext _db;

    // Constructor
    public HoldRepository(AdmissionDbContext db)
    {
        _db = db;
    }

    // Add a new hold
    public void Add(Hold hold)
    {
        _db.Holds.Add(hold);
    }

    // Get hold by ID
    public async Task<Hold?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var hold = await _db.Holds
            .AsNoTracking()
            .Include(h => h.Class)
            .FirstOrDefaultAsync(h => h.Id == id, ct);

        return hold;
    }

    // Get all holds belonging to a student
    public async Task<IReadOnlyList<Hold>> ListByStudentAsync(
        string studentId,
        CancellationToken ct)
    {
        var holds = await _db.Holds
            .AsNoTracking()
            .Include(h => h.Class)
            .Where(h => h.StudentId == studentId)
            .OrderByDescending(h => h.CreatedAt)
            .ToListAsync(ct);

        return holds;
    }

    // Find an active hold for a student in a specific class
    public async Task<Guid?> FindActiveHoldIdAsync(
        Guid classId,
        string studentId,
        DateTime now,
        CancellationToken ct)
    {
        var holdId = await _db.Holds
            .AsNoTracking()
            .Where(h => h.ClassId == classId &&
                        h.StudentId == studentId &&
                        (h.Status == HoldStatus.CONFIRMED ||
                         (h.Status == HoldStatus.HELD && h.ExpiresAt > now)))
            .Select(h => (Guid?)h.Id)
            .FirstOrDefaultAsync(ct);

        return holdId;
    }

    // Count active holds in a class
    public async Task<int> CountActiveAsync(
        Guid classId,
        DateTime now,
        CancellationToken ct)
    {
        var count = await _db.Holds
            .CountAsync(h => h.ClassId == classId &&
                             (h.Status == HoldStatus.CONFIRMED ||
                              (h.Status == HoldStatus.HELD && h.ExpiresAt > now)), ct);

        return count;
    }

    // Expire stale holds for a specific class
    public async Task<int> ExpireStaleForClassAsync(
        Guid classId,
        DateTime now,
        CancellationToken ct)
    {
        var expiredCount = await _db.Holds
            .Where(h => h.ClassId == classId &&
                        h.Status == HoldStatus.HELD &&
                        h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(h => h.Status, HoldStatus.EXPIRED), ct);

        return expiredCount;
    }

    // Expire all stale holds
    public async Task<int> ExpireAllStaleAsync(
        DateTime now,
        CancellationToken ct)
    {
        var expiredCount = await _db.Holds
            .Where(h => h.Status == HoldStatus.HELD &&
                        h.ExpiresAt <= now)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(h => h.Status, HoldStatus.EXPIRED), ct);

        return expiredCount;
    }

    // Confirm a hold
    public async Task<bool> TryConfirmAsync(
        Guid holdId,
        string studentId,
        DateTime now,
        CancellationToken ct)
    {
        var changed = await _db.Holds
            .Where(h => h.Id == holdId &&
                        h.StudentId == studentId &&
                        h.Status == HoldStatus.HELD &&
                        h.ExpiresAt > now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(h => h.Status, HoldStatus.CONFIRMED)
                .SetProperty(h => h.ConfirmedAt, now), ct);

        return changed == 1;
    }

    // Cancel a hold
    public async Task<bool> TryCancelAsync(
        Guid holdId,
        string studentId,
        DateTime now,
        CancellationToken ct)
    {
        var changed = await _db.Holds
            .Where(h => h.Id == holdId &&
                        h.StudentId == studentId &&
                        h.Status == HoldStatus.HELD &&
                        h.ExpiresAt > now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(h => h.Status, HoldStatus.CANCELLED)
                .SetProperty(h => h.CancelledAt, now), ct);

        return changed == 1;
    }
}