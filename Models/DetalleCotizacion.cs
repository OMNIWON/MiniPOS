namespace POS.Models;

public class DetalleCotizacion
{
    public int Id { get; set; }

    public int CotizacionId { get; set; }
    public Cotizacion? Cotizacion { get; set; }

    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public string NombreProducto { get; set; } = string.Empty;
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal DescuentoPorcentaje { get; set; }
    public decimal Subtotal { get; set; }
}