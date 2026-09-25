using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class VentaService : IVentaService
{
    private readonly PosDbContext _context;
    private readonly IInventarioService _inventario;

    public VentaService(PosDbContext context, IInventarioService inventario)
    {
        _context = context;
        _inventario = inventario;
    }

    public async Task<Venta> ConfirmarVentaAsync(List<DetalleVenta> detalles, decimal dineroRecibido, decimal impuestos = 0)
    {
        using var transaccion = await _context.Database.BeginTransactionAsync();
        try
        {
            decimal subtotal = detalles.Sum(d => d.Subtotal);
            decimal total = subtotal + impuestos;

            if (dineroRecibido < total)
                throw new InvalidOperationException("El dinero recibido es menor al total.");

            var venta = new Venta
            {
                Fecha = DateTime.Now,
                Subtotal = subtotal,
                Impuestos = impuestos,
                Total = total,
                DineroRecibido = dineroRecibido,
                Cambio = dineroRecibido - total,
                Detalles = detalles
            };

            foreach (var detalle in detalles)
            {
                bool ok = await _inventario.DisminuirStockAsync(detalle.ProductoId, detalle.Cantidad);
                if (!ok)
                    throw new InvalidOperationException($"No hay stock suficiente de {detalle.NombreProducto}.");
            }

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            await transaccion.CommitAsync();
            return venta;
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    public async Task<List<Venta>> ObtenerHistorialAsync()
    {
        return await _context.Ventas
            .AsNoTracking()
            .Include(v => v.Detalles)
            .OrderByDescending(v => v.Fecha)
            .ToListAsync();
    }

    public async Task<Venta?> ObtenerDetalleAsync(int ventaId)
    {
        return await _context.Ventas
            .AsNoTracking()
            .Include(v => v.Detalles)
            .FirstOrDefaultAsync(v => v.Id == ventaId);
    }
}