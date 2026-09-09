using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lavadero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLogoLavadero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoBase64",
                table: "ConfiguracionLavadero",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LogoContentType",
                table: "ConfiguracionLavadero",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ConfiguracionLavadero",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "LogoBase64", "LogoContentType" },
                values: new object[] { null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoBase64",
                table: "ConfiguracionLavadero");

            migrationBuilder.DropColumn(
                name: "LogoContentType",
                table: "ConfiguracionLavadero");
        }
    }
}
