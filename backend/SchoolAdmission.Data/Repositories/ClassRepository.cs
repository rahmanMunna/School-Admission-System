using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Data.Entities;

namespace SchoolAdmission.Data.Repositories;

public class ClassRepository : IClassRepository
{
    private readonly AdmissionDbContext _db;

    public ClassRepository(AdmissionDbContext db)
    {
        _db = db;
    }

    public void Add(SchoolClass schoolClass)
    {
        _db.Classes.Add(schoolClass);
    }

    // SELECT ... FOR UPDATE: locks this class row until the transaction ends.
    public async Task<SchoolClass?> GetForUpdateAsync(Guid id, CancellationToken ct)
    {
        // ToListAsync (not SingleOrDefaultAsync) so EF sends this SQL exactly as written.
        List<SchoolClass> rows = await _db.Classes
            .FromSql($"SELECT * FROM classes WHERE id = {id} FOR UPDATE")
            .ToListAsync(ct);

        return rows.SingleOrDefault();
    }

    public async Task<IReadOnlyList<ClassSeatCounts>> ListWithSeatCountsAsync(DateTime now, CancellationToken ct)
    {
        // Sort the TABLE first, then turn rows into ClassSeatCounts.
        IQueryable<SchoolClass> sorted = _db.Classes.OrderBy(c => c.Name);
        return await WithSeatCounts(sorted, now).ToListAsync(ct);
    }

    public async Task<ClassSeatCounts?> GetWithSeatCountsAsync(Guid id, DateTime now, CancellationToken ct)
    {
        // Filter the TABLE first, then turn the row into ClassSeatCounts.
        IQueryable<SchoolClass> filtered = _db.Classes.Where(c => c.Id == id);
        return await WithSeatCounts(filtered, now).FirstOrDefaultAsync(ct);
    }

    // Last step: each class row + live counts from the holds table (one SQL query).
    private static IQueryable<ClassSeatCounts> WithSeatCounts(IQueryable<SchoolClass> classes, DateTime now)
    {
        return classes.AsNoTracking().Select(c => new ClassSeatCounts
        {
            Id = c.Id,
            Name = c.Name,
            TotalSeats = c.TotalSeats,
            HeldSeats = c.Holds.Count(h => h.Status == HoldStatus.HELD && h.ExpiresAt > now),
            ConfirmedSeats = c.Holds.Count(h => h.Status == HoldStatus.CONFIRMED)
        });
    }
}