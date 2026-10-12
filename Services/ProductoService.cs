using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;
using System.IO;

namespace POS.Services;

public class ProductoService : IProductoService
{
    private readonly PosDbContext _context;

    public ProductoService(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<Producto>> ObtenerActivosAsync()
    {
        return await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<List<Producto>> ObtenerTodosAsync()
    {
        return await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<List<Producto>> BuscarAsync(string texto)
    {
        texto = texto.Trim().ToLower();
        return await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .Where(p => p.Activo && (
                p.Nombre.ToLower().Contains(texto) ||
                p.Sku.ToLower().Contains(texto)))
            .OrderBy(p => p.Nombre)
            .ToListAsync();
    }

    public async Task<Producto?> ObtenerPorIdAsync(int id)
    {
        return await _context.Productos
            .AsNoTracking()
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Categoria>> ObtenerCategoriasAsync()
    {
        return await _context.Categorias.AsNoTracking().OrderBy(c => c.Nombre).ToListAsync();
    }

    public async Task CrearAsync(Producto producto)
    {
        producto.FechaCreacion = DateTime.Now;
        producto.FechaModificacion = DateTime.Now;
        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
    }

    public async Task EditarAsync(Producto producto)
    {
        producto.FechaModificacion = DateTime.Now;

        var existente = await _context.Productos.FindAsync(producto.Id);
        if (existente is null) return;

        _context.Entry(existente).CurrentValues.SetValues(producto);
        await _context.SaveChangesAsync();
    }

    public async Task DesactivarAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return;
        producto.Activo = false;
        producto.FechaModificacion = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task ActivarAsync(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return;
        producto.Activo = true;
        producto.FechaModificacion = DateTime.Now;
        await _context.SaveChangesAsync();
    }
    public async Task<bool> EliminarAsync(int id)
    {
        bool tieneVentas = await _context.DetallesVenta.AnyAsync(d => d.ProductoId == id);
        bool tieneCotizaciones = await _context.DetallesCotizacion.AnyAsync(d => d.ProductoId == id);

        if (tieneVentas || tieneCotizaciones)
            return false;

        var producto = await _context.Productos.FindAsync(id);
        if (producto is null) return false;

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();
        return true;
    }

    public string GuardarImagen(string rutaOrigen, int productoId)
    {
        string carpetaImagenes = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "POS", "Imagenes");

        Directory.CreateDirectory(carpetaImagenes);

        string extension = Path.GetExtension(rutaOrigen);
        string nombreArchivo = $"producto_{productoId}{extension}";
        string rutaDestino = Path.Combine(carpetaImagenes, nombreArchivo);

        File.Copy(rutaOrigen, rutaDestino, overwrite: true);

        return rutaDestino;
    }

    public async Task<bool> ExisteSkuAsync(string sku, int idExcluir)
    {
        return await _context.Productos
            .AnyAsync(p => p.Sku.ToLower() == sku.ToLower() && p.Id != idExcluir);

    }

    public async Task<string> GenerarSkuAsync(string nombre)
    {
        var palabras = nombre.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        string iniciales = palabras.Length == 1
            ? palabras[0].Substring(0, Math.Min(3, palabras[0].Length)).ToUpper()
            : new string(palabras.Take(2).Select(p => char.ToUpper(p[0])).ToArray());

        if (string.IsNullOrEmpty(iniciales))
            iniciales = "PRD";

        var random = new Random();
        string sku;

        do
        {
            int numero = random.Next(1000, 9999);
            sku = $"{iniciales}-{numero}";
        }
        while (await ExisteSkuAsync(sku, 0));

        return sku;
    }
    public async Task<int> ContarStockBajoAsync()
    {
        var datos = await _context.Productos
            .AsNoTracking()
            .Where(p => p.Activo)
            .Select(p => new { p.Existencia, p.StockMinimo })
            .ToListAsync();

        return datos.Count(p => p.Existencia <= p.StockMinimo);
    }
}