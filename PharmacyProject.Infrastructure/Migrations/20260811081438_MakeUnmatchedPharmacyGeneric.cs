using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeUnmatchedPharmacyGeneric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SourceInsurance",
                table: "UnmatchedPharmacies",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "DataSource",
                table: "UnmatchedPharmacies",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataSource",
                table: "UnmatchedPharmacies");

            migrationBuilder.AlterColumn<int>(
                name: "SourceInsurance",
                table: "UnmatchedPharmacies",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
