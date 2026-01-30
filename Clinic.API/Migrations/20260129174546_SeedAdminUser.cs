using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic.API.Migrations
{
    /// <inheritdoc />
    public partial class SeedAdminUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "PasswordHash", "Role", "StaffId", "Username" },
                values: new object[] { 1, "1BDF7BABB5FB79DD23C5E743207594513236923AC552132A3CE334637215DAE0-E5D489B2753578736047D93A8AB484BF", "superadmin", null, "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
