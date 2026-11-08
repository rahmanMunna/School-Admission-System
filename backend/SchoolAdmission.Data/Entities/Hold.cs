namespace SchoolAdmission.Data.Entities;

// One row per application (seat hold).
public class Hold
{
    public Guid Id { get; set; }

    public Guid ClassId { get; set; }
    public SchoolClass Class { get; set; } = null!;

    public string StudentId { get; set; } = string.Empty;   // 17 digits, always a STRING
    public HoldStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }                 // all times UTC
    public DateTime ExpiresAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}