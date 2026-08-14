using CarDealerManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarDealerManager.Infrastructure.Persistence.Configurations;

public sealed class VehicleCostEntryConfiguration : IEntityTypeConfiguration<VehicleCostEntry>
{
    public void Configure(EntityTypeBuilder<VehicleCostEntry> builder)
    {
        builder.ToTable("vehicle_cost_entries", table =>
        {
            table.HasCheckConstraint(
                "ck_vehicle_cost_entries_non_negative",
                "amount >= 0"
                + " AND (purchase_price_percentage IS NULL OR purchase_price_percentage >= 0)"
                + " AND (tax_percentage IS NULL OR tax_percentage >= 0)"
                + " AND (risk_percentage IS NULL OR risk_percentage >= 0)");
            table.HasCheckConstraint(
                "ck_vehicle_cost_entries_kind_shape",
                "(kind = 'Estimated' AND related_estimate_id IS NULL"
                + " AND (amount > 0 OR purchase_price_percentage > 0))"
                + " OR (kind = 'Actual' AND amount > 0"
                + " AND purchase_price_percentage IS NULL"
                + " AND tax_percentage IS NULL"
                + " AND risk_percentage IS NULL)");
        });

        builder.HasKey(entry => entry.Id).HasName("pk_vehicle_cost_entries");
        builder.Property(entry => entry.Id).HasColumnName("id");
        builder.Property(entry => entry.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(entry => entry.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(entry => entry.ArchivedAtUtc)
            .HasColumnName("archived_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(entry => entry.VehicleId).HasColumnName("vehicle_id");
        builder.Property(entry => entry.Kind)
            .HasColumnName("kind")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(entry => entry.Category)
            .HasColumnName("category")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(entry => entry.Description)
            .HasColumnName("description")
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(entry => entry.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 2);
        builder.Property(entry => entry.PurchasePricePercentage)
            .HasColumnName("purchase_price_percentage")
            .HasPrecision(9, 4);
        builder.Property(entry => entry.TaxPercentage)
            .HasColumnName("tax_percentage")
            .HasPrecision(9, 4);
        builder.Property(entry => entry.RiskPercentage)
            .HasColumnName("risk_percentage")
            .HasPrecision(9, 4);
        builder.Property(entry => entry.EntryDate)
            .HasColumnName("entry_date")
            .HasColumnType("date");
        builder.Property(entry => entry.RelatedEstimateId).HasColumnName("related_estimate_id");
        builder.Property(entry => entry.IncludedInAnalysis)
            .HasColumnName("included_in_analysis");
        builder.Property(entry => entry.Notes).HasColumnName("notes").HasMaxLength(2000);

        builder.HasOne(entry => entry.Vehicle)
            .WithMany(vehicle => vehicle.CostEntries)
            .HasForeignKey(entry => entry.VehicleId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_vehicle_cost_entries_vehicles_vehicle_id");
        builder.HasOne(entry => entry.RelatedEstimate)
            .WithMany(estimate => estimate.RelatedActualCosts)
            .HasForeignKey(entry => entry.RelatedEstimateId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_vehicle_cost_entries_related_estimate_id");

        builder.HasIndex(entry => new { entry.VehicleId, entry.Kind, entry.ArchivedAtUtc })
            .HasDatabaseName("ix_vehicle_cost_entries_vehicle_kind_archived");
        builder.HasIndex(entry => entry.RelatedEstimateId)
            .HasDatabaseName("ix_vehicle_cost_entries_related_estimate_id");
    }
}
