using POS.Models;

namespace POS.Services;

public interface ICotizacionService
{
    Task<Cotizacion> GuardarAsync(List<DetalleCotizacion> detalles, string? cliente);
    Task<List<Cotizacion>> ObtenerHistorialAsync();
    Task<Cotizacion?> ObtenerDetalleAsync(int cotizacionId);
    Task EliminarAsync(int cotizacionId);
}