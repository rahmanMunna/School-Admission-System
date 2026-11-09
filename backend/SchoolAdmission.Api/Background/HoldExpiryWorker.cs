using SchoolAdmission.Business.Services;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;

namespace SchoolAdmission.Api.Background;

// Runs for the whole lifetime of the app.
// Every 5 seconds it marks expired holds as EXPIRED.
public class HoldExpiryWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<HoldExpiryWorker> _logger;

    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    // Constructor
    public HoldExpiryWorker(IServiceScopeFactory scopeFactory,ILogger<HoldExpiryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                // Create a new scope for scoped services.
                using var scope = _scopeFactory.CreateScope();

                var holdService = scope.ServiceProvider
                    .GetRequiredService<IHoldService>();

                var expired = await holdService
                    .ExpireStaleHoldsAsync(stoppingToken);

                if (expired > 0)
                {
                    _logger.LogInformation(
                        "Marked {Count} hold(s) as EXPIRED",
                        expired);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                // Log the error and retry on the next tick.
                _logger.LogWarning(
                    ex,
                    "Expiry sweep failed. Retrying in {Seconds}s.",
                    Interval.TotalSeconds);
            }
        }
    }
}