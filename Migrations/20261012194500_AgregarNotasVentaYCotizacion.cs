using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using POS.Data;

#nullable disable

namespace POS.Migrations;

[DbContext(typeof(PosDbContext))]
[Migration("20261012194500_AgregarNotasVentaYCotizacion")]
public partial class AgregarNotasVentaYCotizacion : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(name: "Nota1", table: "Ventas", type: "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>(name: "Nota2", table: "Ventas", type: "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>(name: "Nota1", table: "Cotizaciones", type: "TEXT", nullable: true);
        migrationBuilder.AddColumn<string>(name: "Nota2", table: "Cotizaciones", type: "TEXT", nullable: true);
        migrationBuilder.Sql("UPDATE Ventas SET Nota1 = Nota WHERE Nota1 IS NULL AND Nota IS NOT NULL AND Nota <> '';");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Nota1", table: "Ventas");
        migrationBuilder.DropColumn(name: "Nota2", table: "Ventas");
        migrationBuilder.DropColumn(name: "Nota1", table: "Cotizaciones");
        migrationBuilder.DropColumn(name: "Nota2", table: "Cotizaciones");
    }
}