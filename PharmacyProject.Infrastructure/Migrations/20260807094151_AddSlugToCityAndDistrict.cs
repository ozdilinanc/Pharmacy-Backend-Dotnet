using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PharmacyProject.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSlugToCityAndDistrict : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Districts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Cities",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Districts");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Cities");
        }
    }
}
