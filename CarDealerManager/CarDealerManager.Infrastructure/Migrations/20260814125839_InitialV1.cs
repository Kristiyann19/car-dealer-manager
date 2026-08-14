using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CarDealerManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "vehicles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    vin = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    mileage = table.Column<int>(type: "integer", nullable: true),
                    engine = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    fuel_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    transmission_type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    source_platform = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    source_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    source_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    physical_location = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    current_bid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    my_bid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    analysis_purchase_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    bid_increment = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 1m),
                    final_purchase_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    purchase_date = table.Column<DateOnly>(type: "date", nullable: true),
                    expected_sale_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    conservative_sale_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    planned_listing_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    minimum_acceptable_sale_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    actual_sale_price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    listing_date = table.Column<DateOnly>(type: "date", nullable: true),
                    sale_date = table.Column<DateOnly>(type: "date", nullable: true),
                    minimum_profit_amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    minimum_roi_percentage = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    archived_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicles", x => x.id);
                    table.CheckConstraint("ck_vehicles_bid_increment", "bid_increment > 0");
                    table.CheckConstraint("ck_vehicles_mileage", "mileage IS NULL OR mileage >= 0");
                    table.CheckConstraint("ck_vehicles_non_negative_amounts", "(current_bid IS NULL OR current_bid >= 0) AND (my_bid IS NULL OR my_bid >= 0) AND (analysis_purchase_price IS NULL OR analysis_purchase_price >= 0) AND (final_purchase_price IS NULL OR final_purchase_price >= 0) AND (expected_sale_price IS NULL OR expected_sale_price >= 0) AND (conservative_sale_price IS NULL OR conservative_sale_price >= 0) AND (planned_listing_price IS NULL OR planned_listing_price >= 0) AND (minimum_acceptable_sale_price IS NULL OR minimum_acceptable_sale_price >= 0) AND (actual_sale_price IS NULL OR actual_sale_price >= 0) AND (minimum_profit_amount IS NULL OR minimum_profit_amount >= 0) AND (minimum_roi_percentage IS NULL OR minimum_roi_percentage >= 0)");
                    table.CheckConstraint("ck_vehicles_year", "year BETWEEN 1886 AND 2200");
                });

            migrationBuilder.CreateTable(
                name: "vehicle_cost_entries",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    vehicle_id = table.Column<int>(type: "integer", nullable: false),
                    kind = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    purchase_price_percentage = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    tax_percentage = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    risk_percentage = table.Column<decimal>(type: "numeric(9,4)", precision: 9, scale: 4, nullable: true),
                    entry_date = table.Column<DateOnly>(type: "date", nullable: true),
                    related_estimate_id = table.Column<int>(type: "integer", nullable: true),
                    included_in_analysis = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    archived_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_vehicle_cost_entries", x => x.id);
                    table.CheckConstraint("ck_vehicle_cost_entries_kind_shape", "(kind = 'Estimated' AND related_estimate_id IS NULL AND (amount > 0 OR purchase_price_percentage > 0)) OR (kind = 'Actual' AND amount > 0 AND purchase_price_percentage IS NULL AND tax_percentage IS NULL AND risk_percentage IS NULL)");
                    table.CheckConstraint("ck_vehicle_cost_entries_non_negative", "amount >= 0 AND (purchase_price_percentage IS NULL OR purchase_price_percentage >= 0) AND (tax_percentage IS NULL OR tax_percentage >= 0) AND (risk_percentage IS NULL OR risk_percentage >= 0)");
                    table.ForeignKey(
                        name: "fk_vehicle_cost_entries_related_estimate_id",
                        column: x => x.related_estimate_id,
                        principalTable: "vehicle_cost_entries",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_vehicle_cost_entries_vehicles_vehicle_id",
                        column: x => x.vehicle_id,
                        principalTable: "vehicles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_cost_entries_related_estimate_id",
                table: "vehicle_cost_entries",
                column: "related_estimate_id");

            migrationBuilder.CreateIndex(
                name: "ix_vehicle_cost_entries_vehicle_kind_archived",
                table: "vehicle_cost_entries",
                columns: new[] { "vehicle_id", "kind", "archived_at_utc" });

            migrationBuilder.CreateIndex(
                name: "ix_vehicles_archived_status",
                table: "vehicles",
                columns: new[] { "archived_at_utc", "status" });

            migrationBuilder.CreateIndex(
                name: "ux_vehicles_vin",
                table: "vehicles",
                column: "vin",
                unique: true,
                filter: "vin IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "vehicle_cost_entries");

            migrationBuilder.DropTable(
                name: "vehicles");
        }
    }
}
