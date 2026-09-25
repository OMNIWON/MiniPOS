using POS.Models;

namespace POS.Services;

public interface IVentaService
{
    Task<Venta> ConfirmarVentaAsync(List<DetalleVenta> detalles, decimal dineroRecibido, decimal impuestos = 0);
    Task<List<Venta>> ObtenerHistorialAsync();
    Task<Venta?> ObtenerDetalleAsync(int ventaId);
}