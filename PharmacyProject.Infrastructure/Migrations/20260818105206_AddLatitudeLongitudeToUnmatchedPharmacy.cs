using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLatitudeLongitudeToUnmatchedPharmacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "ScrapedLatitude",
                table: "UnmatchedPharmacies",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ScrapedLongitude",
                table: "UnmatchedPharmacies",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScrapedLatitude",
                table: "UnmatchedPharmacies");

            migrationBuilder.DropColumn(
                name: "ScrapedLongitude",
                table: "UnmatchedPharmacies");
        }
    }
}
