using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLocationToUnmatchedPharmacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "UnmatchedPharmacies",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "UnmatchedPharmacies",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityId",
                table: "UnmatchedPharmacies");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "UnmatchedPharmacies");
        }
    }
}
