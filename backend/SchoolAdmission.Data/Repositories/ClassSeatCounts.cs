namespace SchoolAdmission.Data.Repositories;

// Read model: one class with its live seat counts (counted by the database, not stored).
public class ClassSeatCounts
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int TotalSeats { get; set; }
    public int HeldSeats { get; set; }
    public int ConfirmedSeats { get; set; }
}