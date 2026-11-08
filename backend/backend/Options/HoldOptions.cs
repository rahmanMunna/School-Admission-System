namespace SchoolAdmission.Api.Options;

// Filled from the HOLD_DURATION_SECONDS environment variable (wired up in Step 3).
public sealed class HoldOptions
{
    public const int DefaultSeconds = 600;
    public int HoldDurationSeconds { get; set; } = DefaultSeconds;
}