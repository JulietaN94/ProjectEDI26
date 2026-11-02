using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Lavadero.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AgregarDatosDemo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Activo", "Codigo", "Descripcion", "Imagen", "ImagenContentType", "Marca", "Nombre", "PrecioVenta" },
                values: new object[,]
                {
                    { -1012, true, "DEMO-012", "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.", null, null, "CleanPro", "Desengrasante multiuso", 8900m },
                    { -1011, true, "DEMO-011", "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.", null, null, "AutoCare", "Kit de detailing básico", 29500m },
                    { -1010, true, "DEMO-010", "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.", null, null, "Lavadero Test", "Aromatizante interior", 5200m },
                    { -1009, true, "DEMO-009", "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.", null, null, "DetailLab", "Sellador de pintura", 18500m },
                    { -1008, true, "DEMO-008", "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.", null, null, "BrilloMax", "Esponja de lavado", 3900m },
                    { -1007, true, "DEMO-007", "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.", null, null, "AutoCare", "Cepillo para llantas", 11300m },
                    { -1006, true, "DEMO-006", "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.", null, null, "CleanPro", "Limpiavidrios 500 ml", 6900m },
                    { -1005, true, "DEMO-005", "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.", null, null, "Lavadero Test", "Paño de microfibra", 4200m },
                    { -1004, true, "DEMO-004", "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.", null, null, "DetailLab", "Revividor de neumáticos", 9900m },
                    { -1003, true, "DEMO-003", "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.", null, null, "CleanPro", "Limpiador de interiores", 7800m },
                    { -1002, true, "DEMO-002", "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.", null, null, "AutoCare", "Cera líquida 500 ml", 12500m },
                    { -1001, true, "DEMO-001", "Producto de demostración incluido para presentar el catálogo y el flujo de pedidos.", null, null, "BrilloMax", "Shampoo neutro 500 ml", 8500m }
                });

            migrationBuilder.InsertData(
                table: "Servicios",
                columns: new[] { "Id", "Activo", "Descripcion", "DuracionMinutos", "Nombre", "Precio" },
                values: new object[,]
                {
                    { -107, true, "Limpieza de bajos y chasis del vehículo.", 60, "Lavado de chasis", 20000m },
                    { -106, true, "Remoción de contaminantes adheridos a la carrocería.", 90, "Descontaminado de pintura", 28000m },
                    { -105, true, "Protección acrílica para la pintura del vehículo.", 150, "Tratamiento acrílico", 48000m },
                    { -104, true, "Recuperación y protección de ópticas delanteras.", 60, "Pulido de ópticas", 19000m },
                    { -103, true, "Limpieza profunda de asientos y alfombras.", 120, "Limpieza de tapizados", 32000m },
                    { -102, true, "Aplicación manual de cera protectora.", 60, "Encerado manual", 22000m },
                    { -101, true, "Limpieza exterior del compartimiento del motor.", 45, "Lavado de motor", 16000m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: -1012);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: -1011);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: -1010);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: -1009);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: -1008);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: -1007);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: -1006);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: -1005);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: -1004);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: -1003);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: -1002);

            migrationBuilder.DeleteData(
                table: "Productos",
                keyColumn: "Id",
                keyValue: -1001);

            migrationBuilder.DeleteData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: -107);

            migrationBuilder.DeleteData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: -106);

            migrationBuilder.DeleteData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: -105);

            migrationBuilder.DeleteData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: -104);

            migrationBuilder.DeleteData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: -103);

            migrationBuilder.DeleteData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: -102);

            migrationBuilder.DeleteData(
                table: "Servicios",
                keyColumn: "Id",
                keyValue: -101);
        }
    }
}
