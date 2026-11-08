namespace SchoolAdmission.Business.Options;

// Filled from the HOLD_DURATION_SECONDS environment variable (wired up in Program.cs later).
public sealed class HoldOptions
{
    public const int DefaultSeconds = 600;
    public int HoldDurationSeconds { get; set; } = DefaultSeconds;
}