using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Clinic.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMedicalNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalNotes_Patients_PatientId",
                table: "MedicalNotes");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "OtherMedicalNotes",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "MedicalNotes");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "MedicalNotes",
                newName: "AppointmentId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalNotes_PatientId",
                table: "MedicalNotes",
                newName: "IX_MedicalNotes_AppointmentId");

            migrationBuilder.AddColumn<string>(
                name: "Diagnosis",
                table: "MedicalNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "MedicalNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prescription",
                table: "MedicalNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Symptoms",
                table: "MedicalNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Treatment",
                table: "MedicalNotes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalNotes_Appointments_AppointmentId",
                table: "MedicalNotes",
                column: "AppointmentId",
                principalTable: "Appointments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MedicalNotes_Appointments_AppointmentId",
                table: "MedicalNotes");

            migrationBuilder.DropColumn(
                name: "Diagnosis",
                table: "MedicalNotes");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "MedicalNotes");

            migrationBuilder.DropColumn(
                name: "Prescription",
                table: "MedicalNotes");

            migrationBuilder.DropColumn(
                name: "Symptoms",
                table: "MedicalNotes");

            migrationBuilder.DropColumn(
                name: "Treatment",
                table: "MedicalNotes");

            migrationBuilder.RenameColumn(
                name: "AppointmentId",
                table: "MedicalNotes",
                newName: "PatientId");

            migrationBuilder.RenameIndex(
                name: "IX_MedicalNotes_AppointmentId",
                table: "MedicalNotes",
                newName: "IX_MedicalNotes_PatientId");

            migrationBuilder.AddColumn<decimal>(
                name: "Height",
                table: "Patients",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherMedicalNotes",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Weight",
                table: "Patients",
                type: "decimal(5,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "MedicalNotes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_MedicalNotes_Patients_PatientId",
                table: "MedicalNotes",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
