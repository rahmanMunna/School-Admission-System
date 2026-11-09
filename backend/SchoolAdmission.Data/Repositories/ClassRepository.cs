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
        await WithSeatCounts(db.Classes.OrderBy(c => c.Name), now)      // sort the TABLE first
            .ToListAsync(ct);

    public Task<ClassSeatCounts?> GetWithSeatCountsAsync(Guid id, DateTime now, CancellationToken ct) =>
        WithSeatCounts(db.Classes.Where(c => c.Id == id), now)          // filter the TABLE first
            .FirstOrDefaultAsync(ct);

    // Last step: turn each class row into ClassSeatCounts (with live counts from the holds table).
    private static IQueryable<ClassSeatCounts> WithSeatCounts(IQueryable<SchoolClass> classes, DateTime now) =>
        classes.AsNoTracking().Select(c => new ClassSeatCounts(
            c.Id,
            c.Name,
            c.TotalSeats,
            c.Holds.Count(h => h.Status == HoldStatus.HELD && h.ExpiresAt > now),
            c.Holds.Count(h => h.Status == HoldStatus.CONFIRMED)));
}