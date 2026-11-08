using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolAdmission.Data.Entities;

namespace SchoolAdmission.Data.Configurations;

public class SchoolClassConfiguration : IEntityTypeConfiguration<SchoolClass>
{
    public void Configure(EntityTypeBuilder<SchoolClass> builder)
    {
        builder.ToTable("classes", t =>
        {
            t.HasCheckConstraint("ck_classes_total_seats_positive", "total_seats > 0");
            t.HasCheckConstraint("ck_classes_name_not_blank", "length(btrim(name)) > 0");
        });

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();          // we create Guids in code

        builder.Property(c => c.Name).HasMaxLength(100).IsRequired();
        builder.Property(c => c.TotalSeats).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();

        builder.HasMany(c => c.Holds)
               .WithOne(h => h.Class)
               .HasForeignKey(h => h.ClassId)
               .OnDelete(DeleteBehavior.Restrict);                  // never delete a class that has holds
    }
}