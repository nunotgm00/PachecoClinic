using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PachecoClinic.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIntervetionOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Interventions_AppointmentId",
                table: "Interventions");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_AppointmentId",
                table: "Interventions",
                column: "AppointmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Interventions_AppointmentId",
                table: "Interventions");

            migrationBuilder.CreateIndex(
                name: "IX_Interventions_AppointmentId",
                table: "Interventions",
                column: "AppointmentId");
        }
    }
}
