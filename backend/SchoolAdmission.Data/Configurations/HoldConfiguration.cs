using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SchoolAdmission.Data.Entities;

namespace SchoolAdmission.Data.Configurations;

public class HoldConfiguration : IEntityTypeConfiguration<Hold>
{
    public void Configure(EntityTypeBuilder<Hold> builder)
    {
        builder.ToTable("holds", t =>
        {
            t.HasCheckConstraint("ck_holds_student_id_17_digits", "student_id ~ '^[0-9]{17}$'");
            t.HasCheckConstraint("ck_holds_status_valid",
                "status IN ('HELD', 'CONFIRMED', 'EXPIRED', 'CANCELLED')");
            t.HasCheckConstraint("ck_holds_expires_after_created", "expires_at > created_at");
        });

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever();

        builder.Property(h => h.StudentId).HasMaxLength(17).IsRequired();

        builder.Property(h => h.Status)
               .HasConversion<string>()                             // stored as 'HELD', not 0
               .HasMaxLength(16)
               .IsRequired();

        builder.Property(h => h.CreatedAt).IsRequired();
        builder.Property(h => h.ExpiresAt).IsRequired();

        // RULE: one ACTIVE application per student per class (DB-level safety net)
        builder.HasIndex(h => new { h.ClassId, h.StudentId })
               .IsUnique()
               .HasFilter("status IN ('HELD', 'CONFIRMED')")
               .HasDatabaseName("ux_holds_active_student_class");

        // Fast seat counting per class
        builder.HasIndex(h => new { h.ClassId, h.Status, h.ExpiresAt })
               .HasDatabaseName("ix_holds_class_status_expires");

        // Fast "My applications" (newest first)
        builder.HasIndex(h => new { h.StudentId, h.CreatedAt })
               .IsDescending(false, true)
               .HasDatabaseName("ix_holds_student_created");

        // Fast expiry sweeper
        builder.HasIndex(h => h.ExpiresAt)
               .HasFilter("status = 'HELD'")
               .HasDatabaseName("ix_holds_held_expires");
    }
}