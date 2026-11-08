using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Data.Entities;

namespace SchoolAdmission.Data.Repositories;

public class ClassRepository(AdmissionDbContext db) : IClassRepository
{
    public void Add(SchoolClass schoolClass) => db.Classes.Add(schoolClass);

    public async Task<SchoolClass?> GetForUpdateAsync(Guid id, CancellationToken ct)
    {
        // ToListAsync (not SingleOrDefaultAsync) so EF sends this SQL exactly as written.
        var rows = await db.Classes
            .FromSql($"SELECT * FROM classes WHERE id = {id} FOR UPDATE")
            .ToListAsync(ct);

        return rows.SingleOrDefault();
    }

    public async Task<IReadOnlyList<ClassSeatCounts>> ListWithSeatCountsAsync(DateTime now, CancellationToken ct) =>
        await SeatCountsQuery(now)
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

    public Task<ClassSeatCounts?> GetWithSeatCountsAsync(Guid id, DateTime now, CancellationToken ct) =>
        SeatCountsQuery(now)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync(ct);

    // One query: each class + COUNT of active HELD + COUNT of CONFIRMED
    private IQueryable<ClassSeatCounts> SeatCountsQuery(DateTime now) =>
        db.Classes.AsNoTracking().Select(c => new ClassSeatCounts(
            c.Id,
            c.Name,
            c.TotalSeats,
            c.Holds.Count(h => h.Status == HoldStatus.HELD && h.ExpiresAt > now),
            c.Holds.Count(h => h.Status == HoldStatus.CONFIRMED)));
}