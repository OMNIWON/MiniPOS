using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace POS.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNotasDobles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nota1",
                table: "Ventas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nota2",
                table: "Ventas",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nota1",
                table: "Cotizaciones",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Nota2",
                table: "Cotizaciones",
                type: "TEXT",
                nullable: true);

            migrationBuilder.Sql("UPDATE Ventas SET Nota1 = Nota WHERE Nota IS NOT NULL AND Nota <> '';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nota1",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Nota2",
                table: "Ventas");

            migrationBuilder.DropColumn(
                name: "Nota1",
                table: "Cotizaciones");

            migrationBuilder.DropColumn(
                name: "Nota2",
                table: "Cotizaciones");
        }
    }
}
