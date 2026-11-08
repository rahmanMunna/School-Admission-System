using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SchoolAdmission.Data.Repositories;

namespace SchoolAdmission.Data;

public static class DataServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        // NOTE: no EnableRetryOnFailure(). It conflicts with our manual transactions
        // (the lock → count → insert flow).
        services.AddDbContext<AdmissionDbContext>(options =>
            options.UseNpgsql(connectionString)
                   .UseSnakeCaseNamingConvention());

        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IHoldRepository, HoldRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}