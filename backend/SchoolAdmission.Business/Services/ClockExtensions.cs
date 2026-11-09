namespace SchoolAdmission.Business.Services;

public static class ClockExtensions
{
    public static DateTime UtcNowSeconds(this TimeProvider clock)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        return new DateTime(now.Ticks - now.Ticks % TimeSpan.TicksPerSecond, DateTimeKind.Utc);
    }
}