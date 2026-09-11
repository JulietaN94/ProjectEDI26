using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lavadero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTokenCancelacionTurnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TokenCancelacion",
                table: "Turnos",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_TokenCancelacion",
                table: "Turnos",
                column: "TokenCancelacion",
                unique: true,
                filter: "[TokenCancelacion] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Turnos_TokenCancelacion",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "TokenCancelacion",
                table: "Turnos");
        }
    }
}
