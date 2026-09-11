using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lavadero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarConfirmacionTurnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaConfirmacion",
                table: "Turnos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaExpiracionConfirmacion",
                table: "Turnos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenConfirmacion",
                table: "Turnos",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Turnos_TokenConfirmacion",
                table: "Turnos",
                column: "TokenConfirmacion",
                unique: true,
                filter: "[TokenConfirmacion] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Turnos_TokenConfirmacion",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "FechaConfirmacion",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "FechaExpiracionConfirmacion",
                table: "Turnos");

            migrationBuilder.DropColumn(
                name: "TokenConfirmacion",
                table: "Turnos");
        }
    }
}
