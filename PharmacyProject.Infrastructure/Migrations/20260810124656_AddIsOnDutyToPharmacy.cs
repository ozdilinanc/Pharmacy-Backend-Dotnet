using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIsOnDutyToPharmacy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnDuty",
                table: "Pharmacies",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOnDuty",
                table: "Pharmacies");
        }
    }
}
