using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SchoolAdmission.Data;

// Used ONLY at design time (Add-Migration). The running app gets its
// connection string from the environment variable instead.
// Add-Migration does not connect to the database, so this value can be a placeholder.
public class AdmissionDbContextFactory : IDesignTimeDbContextFactory<AdmissionDbContext>
{
    public AdmissionDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? "Host=localhost;Port=5432;Database=admission;Username=admission;Password=admission";

        var options = new DbContextOptionsBuilder<AdmissionDbContext>()
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .Options;

        return new AdmissionDbContext(options);
    }
}