using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic.API.Migrations
{
    /// <inheritdoc />
    public partial class AddTaxRatio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Ratio",
                table: "Taxes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Taxes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Taxes",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Ratio", "Status" },
                values: new object[] { 0m, 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ratio",
                table: "Taxes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Taxes");
        }
    }
}
