using Microsoft.EntityFrameworkCore;
using SchoolAdmission.Data.Entities;

namespace SchoolAdmission.Data;

public class AdmissionDbContext(DbContextOptions<AdmissionDbContext> options) : DbContext(options)
{
    public DbSet<SchoolClass> Classes => Set<SchoolClass>();
    public DbSet<Hold> Holds => Set<Hold>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Picks up SchoolClassConfiguration and HoldConfiguration automatically
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AdmissionDbContext).Assembly);
    }
}