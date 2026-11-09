using SchoolAdmission.Business.Services;

namespace SchoolAdmission.Api.Background;

// Runs for the whole lifetime of the app. Every 5 seconds it marks expired holds as EXPIRED.
public class HoldExpiryWorker(IServiceScopeFactory scopeFactory, ILogger<HoldExpiryWorker> logger): BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                // The service and DbContext are "scoped", so create a fresh scope each time.
                using var scope = scopeFactory.CreateScope();
                var holdService = scope.ServiceProvider.GetRequiredService<IHoldService>();

                var expired = await holdService.ExpireStaleHoldsAsync(stoppingToken);
                if (expired > 0)
                    logger.LogInformation("Marked {Count} hold(s) as EXPIRED", expired);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Never let an error stop the app. Just try again on the next tick.
                logger.LogWarning(ex, "Expiry sweep failed. Retrying in {Seconds}s.", Interval.TotalSeconds);
            }
        }
    }
}