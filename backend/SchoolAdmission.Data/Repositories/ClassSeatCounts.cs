namespace SchoolAdmission.Data.Repositories;

// Read model: one class with its live seat counts (computed by the DB, not stored).
public record ClassSeatCounts(
    Guid Id,
    string Name,
    int TotalSeats,
    int HeldSeats,
    int ConfirmedSeats);