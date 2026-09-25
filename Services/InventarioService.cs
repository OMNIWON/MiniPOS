using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class InventarioService : IInventarioService
{
    private readonly PosDbContext _context;

    public InventarioService(PosDbContext context)
    {
        _context = context;
    }

    public async Task AumentarStockAsync(int productoId, decimal cantidad)
    {
        var producto = await _context.Productos.FindAsync(productoId);
        if (producto is null) return;
        producto.Existencia += cantidad;
        producto.FechaModificacion = DateTime.Now;
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DisminuirStockAsync(int productoId, decimal cantidad)
    {
        var producto = await _context.Productos.FindAsync(productoId);
        if (producto is null) return false;

        producto.Existencia -= cantidad;
        producto.FechaModificacion = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HayStockSuficienteAsync(int productoId, decimal cantidad)
    {
        var producto = await _context.Productos.FindAsync(productoId);
        return producto is not null && producto.Existencia >= cantidad;
    }

    public async Task<List<Producto>> ObtenerBajoStockMinimoAsync()
    {
        return await _context.Productos
            .Where(p => p.Activo && p.Existencia <= p.StockMinimo)
            .ToListAsync();
    }
}