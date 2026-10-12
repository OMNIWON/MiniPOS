using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class CotizacionService : ICotizacionService
{
    private readonly PosDbContext _context;

    public CotizacionService(PosDbContext context)
    {
        _context = context;
    }

    public async Task<Cotizacion> GuardarAsync(List<DetalleCotizacion> detalles, string? cliente)
    {
        var cotizacion = new Cotizacion
        {
            Fecha = DateTime.Now,
            Cliente = cliente,
            Total = detalles.Sum(d => d.Subtotal),
            Detalles = detalles
        };

        _context.Cotizaciones.Add(cotizacion);
        await _context.SaveChangesAsync();
        return cotizacion;
    }

    public async Task<List<Cotizacion>> ObtenerHistorialAsync()
    {
        return await _context.Cotizaciones
            .AsNoTracking()
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();
    }

    public async Task<Cotizacion?> ObtenerDetalleAsync(int cotizacionId)
    {
        return await _context.Cotizaciones
            .AsNoTracking()
            .Include(c => c.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.Id == cotizacionId);
    }
    public async Task GuardarNotasAsync(int cotizacionId, string? nota1, string? nota2)
    {
        var cotizacion = await _context.Cotizaciones.FirstOrDefaultAsync(c => c.Id == cotizacionId);
        if (cotizacion is null)
            throw new InvalidOperationException("La cotización ya no existe en el historial.");

        cotizacion.Nota1 = nota1;
        cotizacion.Nota2 = nota2;
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAsync(int cotizacionId)
    {
        var cotizacion = await _context.Cotizaciones.FindAsync(cotizacionId);
        if (cotizacion is null) return;

        _context.Cotizaciones.Remove(cotizacion);
        await _context.SaveChangesAsync();
    }
}