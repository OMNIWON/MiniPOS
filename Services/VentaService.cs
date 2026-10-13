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

    public async Task<Venta> ConfirmarVentaAsync(List<DetalleVenta> detalles, decimal dineroRecibido,
        MetodoPago metodoPago = MetodoPago.Efectivo, DesgloseVenta? desglose = null)
    {
        using var transaccion = await _context.Database.BeginTransactionAsync();
        try
        {
            decimal suma = detalles.Sum(d => d.Subtotal);
            var calculo = desglose ?? new DesgloseVenta(suma, 0m, suma, 0m);

            if (dineroRecibido < calculo.Total)
                throw new InvalidOperationException("El dinero recibido es menor al total.");

            var venta = new Venta
            {
                Fecha = DateTime.Now,
                Subtotal = calculo.Subtotal,
                Impuestos = calculo.Impuestos,
                TasaIva = calculo.TasaIva,
                Total = calculo.Total,
                DineroRecibido = dineroRecibido,
                Cambio = dineroRecibido - calculo.Total,
                MetodoPago = metodoPago,
                Detalles = detalles
            };

            foreach (var detalle in detalles)
            {
                bool ok = await _inventario.DisminuirStockAsync(detalle.ProductoId, detalle.Cantidad);
                if (!ok)
                    throw new InvalidOperationException($"No se pudo descontar el inventario de {detalle.NombreProducto}.");
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

    public async Task AnularVentaAsync(int ventaId)
    {
        using var transaccion = await _context.Database.BeginTransactionAsync();
        try
        {
            var venta = await _context.Ventas
                .Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == ventaId);

            if (venta is null)
                throw new InvalidOperationException("La venta no existe.");

            foreach (var detalle in venta.Detalles)
            {
                await _inventario.AumentarStockAsync(detalle.ProductoId, detalle.Cantidad);
            }

            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();

            await transaccion.CommitAsync();
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }
    public async Task GuardarNotasAsync(int ventaId, string? nota1, string? nota2)
    {
        var venta = await _context.Ventas.FirstOrDefaultAsync(v => v.Id == ventaId);
        if (venta is null)
            throw new InvalidOperationException("La venta ya no existe en el historial.");

        venta.Nota1 = nota1;
        venta.Nota2 = nota2;
        await _context.SaveChangesAsync();
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

    public async Task<decimal> ObtenerTotalVentasHoyAsync()
    {
        var hoy = DateTime.Today;
        var manana = hoy.AddDays(1);

        var totales = await _context.Ventas
            .AsNoTracking()
            .Where(v => v.Fecha >= hoy && v.Fecha < manana)
            .Select(v => v.Total)
            .ToListAsync();

        return totales.Sum();
    }
}