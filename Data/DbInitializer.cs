using POS.Models;

namespace POS.Data;

public static class DbInitializer
{
    public static void Seed(PosDbContext context)
    {
        if (context.Productos.Any()) return;

        var catGas = new Categoria { Nombre = "Gas LP" };
        var catFerreteria = new Categoria { Nombre = "Ferretería" };
        context.Categorias.AddRange(catGas, catFerreteria);
        context.SaveChanges();

        context.Productos.AddRange(
            new Producto
            {
                Sku = "QUE-001",
                Nombre = "Quemador industrial",
                CategoriaId = catGas.Id,
                Unidad = UnidadMedida.Pieza,
                PrecioCompra = 1200m,
                PrecioVenta = 1850m,
                Existencia = 4,
                StockMinimo = 1
            },
            new Producto
            {
                Sku = "REG-001",
                Nombre = "Regulador de gas",
                CategoriaId = catGas.Id,
                Unidad = UnidadMedida.Pieza,
                PrecioCompra = 160m,
                PrecioVenta = 250m,
                Existencia = 15,
                StockMinimo = 3
            },
            new Producto
            {
                Sku = "COD-001",
                Nombre = "Codo de cobre",
                CategoriaId = catFerreteria.Id,
                Unidad = UnidadMedida.Pieza,
                PrecioCompra = 15m,
                PrecioVenta = 28m,
                Existencia = 60,
                StockMinimo = 10
            },
            new Producto
            {
                Sku = "MAN-001",
                Nombre = "Manguera Coflex",
                CategoriaId = catFerreteria.Id,
                Unidad = UnidadMedida.Metro,
                PrecioCompra = 20m,
                PrecioVenta = 35m,
                Existencia = 50,
                StockMinimo = 5
            }
        );

        context.SaveChanges();
    }
}