using POS.Models;

namespace POS.Services;

public interface IVentaService
{
    Task<Venta> ConfirmarVentaAsync(List<DetalleVenta> detalles, decimal dineroRecibido,
        MetodoPago metodoPago = MetodoPago.Efectivo, DesgloseVenta? desglose = null);
    Task<List<Venta>> ObtenerHistorialAsync();
    Task<Venta?> ObtenerDetalleAsync(int ventaId);
    Task AnularVentaAsync(int ventaId);
    Task<decimal> ObtenerTotalVentasHoyAsync();
    Task GuardarNotasAsync(int ventaId, string? nota1, string? nota2);
}