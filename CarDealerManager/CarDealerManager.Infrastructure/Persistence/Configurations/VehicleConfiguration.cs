using CarDealerManager.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarDealerManager.Infrastructure.Persistence.Configurations;

public sealed class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("vehicles", table =>
        {
            table.HasCheckConstraint("ck_vehicles_year", "year BETWEEN 1886 AND 2200");
            table.HasCheckConstraint("ck_vehicles_mileage", "mileage IS NULL OR mileage >= 0");
            table.HasCheckConstraint("ck_vehicles_bid_increment", "bid_increment > 0");
            table.HasCheckConstraint(
                "ck_vehicles_non_negative_amounts",
                "(current_bid IS NULL OR current_bid >= 0)"
                + " AND (my_bid IS NULL OR my_bid >= 0)"
                + " AND (analysis_purchase_price IS NULL OR analysis_purchase_price >= 0)"
                + " AND (final_purchase_price IS NULL OR final_purchase_price >= 0)"
                + " AND (expected_sale_price IS NULL OR expected_sale_price >= 0)"
                + " AND (conservative_sale_price IS NULL OR conservative_sale_price >= 0)"
                + " AND (planned_listing_price IS NULL OR planned_listing_price >= 0)"
                + " AND (minimum_acceptable_sale_price IS NULL OR minimum_acceptable_sale_price >= 0)"
                + " AND (actual_sale_price IS NULL OR actual_sale_price >= 0)"
                + " AND (minimum_profit_amount IS NULL OR minimum_profit_amount >= 0)"
                + " AND (minimum_roi_percentage IS NULL OR minimum_roi_percentage >= 0)");
        });

        builder.HasKey(vehicle => vehicle.Id).HasName("pk_vehicles");
        builder.Property(vehicle => vehicle.Id).HasColumnName("id");
        builder.Property(vehicle => vehicle.CreatedAtUtc)
            .HasColumnName("created_at_utc")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(vehicle => vehicle.UpdatedAtUtc)
            .HasColumnName("updated_at_utc")
            .HasColumnType("timestamp with time zone")
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(vehicle => vehicle.ArchivedAtUtc)
            .HasColumnName("archived_at_utc")
            .HasColumnType("timestamp with time zone");

        builder.Property(vehicle => vehicle.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
        builder.Property(vehicle => vehicle.Brand)
            .HasColumnName("brand")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(vehicle => vehicle.Model)
            .HasColumnName("model")
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(vehicle => vehicle.Year).HasColumnName("year").IsRequired();
        builder.Property(vehicle => vehicle.Vin).HasColumnName("vin").HasMaxLength(50);
        builder.Property(vehicle => vehicle.Mileage).HasColumnName("mileage");
        builder.Property(vehicle => vehicle.Engine).HasColumnName("engine").HasMaxLength(100);
        builder.Property(vehicle => vehicle.FuelType)
            .HasColumnName("fuel_type")
            .HasConversion<string>()
            .HasMaxLength(30);
        builder.Property(vehicle => vehicle.TransmissionType)
            .HasColumnName("transmission_type")
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(vehicle => vehicle.SourcePlatform)
            .HasColumnName("source_platform")
            .HasMaxLength(100);
        builder.Property(vehicle => vehicle.SourceUrl)
            .HasColumnName("source_url")
            .HasMaxLength(2048);
        builder.Property(vehicle => vehicle.SourceCountry)
            .HasColumnName("source_country")
            .HasMaxLength(100);
        builder.Property(vehicle => vehicle.PhysicalLocation)
            .HasColumnName("physical_location")
            .HasMaxLength(200);

        ConfigureMoney(builder.Property(vehicle => vehicle.CurrentBid), "current_bid");
        ConfigureMoney(builder.Property(vehicle => vehicle.MyBid), "my_bid");
        ConfigureMoney(builder.Property(vehicle => vehicle.AnalysisPurchasePrice), "analysis_purchase_price");
        ConfigureMoney(builder.Property(vehicle => vehicle.BidIncrement), "bid_increment");
        builder.Property(vehicle => vehicle.BidIncrement).HasDefaultValue(1m);
        ConfigureMoney(builder.Property(vehicle => vehicle.FinalPurchasePrice), "final_purchase_price");
        builder.Property(vehicle => vehicle.PurchaseDate).HasColumnName("purchase_date").HasColumnType("date");

        ConfigureMoney(builder.Property(vehicle => vehicle.ExpectedSalePrice), "expected_sale_price");
        ConfigureMoney(builder.Property(vehicle => vehicle.ConservativeSalePrice), "conservative_sale_price");
        ConfigureMoney(builder.Property(vehicle => vehicle.PlannedListingPrice), "planned_listing_price");
        ConfigureMoney(
            builder.Property(vehicle => vehicle.MinimumAcceptableSalePrice),
            "minimum_acceptable_sale_price");
        ConfigureMoney(builder.Property(vehicle => vehicle.ActualSalePrice), "actual_sale_price");
        builder.Property(vehicle => vehicle.ListingDate).HasColumnName("listing_date").HasColumnType("date");
        builder.Property(vehicle => vehicle.SaleDate).HasColumnName("sale_date").HasColumnType("date");

        ConfigureMoney(builder.Property(vehicle => vehicle.MinimumProfitAmount), "minimum_profit_amount");
        builder.Property(vehicle => vehicle.MinimumRoiPercentage)
            .HasColumnName("minimum_roi_percentage")
            .HasPrecision(9, 4);

        builder.HasIndex(vehicle => vehicle.Vin)
            .IsUnique()
            .HasFilter("vin IS NOT NULL")
            .HasDatabaseName("ux_vehicles_vin");
        builder.HasIndex(vehicle => new { vehicle.ArchivedAtUtc, vehicle.Status })
            .HasDatabaseName("ix_vehicles_archived_status");
    }

    private static void ConfigureMoney<TProperty>(
        PropertyBuilder<TProperty> property,
        string columnName)
        => property.HasColumnName(columnName).HasPrecision(18, 2);
}
