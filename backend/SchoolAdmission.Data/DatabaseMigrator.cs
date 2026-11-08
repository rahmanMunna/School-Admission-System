using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace SchoolAdmission.Data;

// Applies EF migrations when the app starts, so "docker compose up" creates the tables automatically.
// Retries because the database container can need a few seconds before it accepts connections.
public static class DatabaseMigrator
{
    public static async Task MigrateDatabaseAsync(this IServiceProvider services, ILogger logger,
                                                  CancellationToken ct = default)
    {
        const int maxAttempts = 10;

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var scope = services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<AdmissionDbContext>();
                await db.Database.MigrateAsync(ct);
                logger.LogInformation("Database is up to date.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts && IsDatabaseUnavailable(ex))
            {
                logger.LogWarning("Database not ready (attempt {Attempt}/{Max}): {Message}. Retrying in 2s…",
                    attempt, maxAttempts, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(2), ct);
            }
        }
    }

    private static bool IsDatabaseUnavailable(Exception ex) =>
        ex is NpgsqlException or TimeoutException || ex.InnerException is NpgsqlException;
}