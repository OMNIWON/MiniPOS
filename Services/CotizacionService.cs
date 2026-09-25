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
            .OrderByDescending(c => c.Fecha)
            .ToListAsync();
    }

    public async Task<Cotizacion?> ObtenerDetalleAsync(int cotizacionId)
    {
        return await _context.Cotizaciones
            .AsNoTracking()
            .Include(c => c.Detalles)
            .FirstOrDefaultAsync(c => c.Id == cotizacionId);
    }
}