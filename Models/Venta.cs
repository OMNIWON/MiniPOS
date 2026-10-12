namespace POS.Models;

public class Venta
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;

    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal TasaIva { get; set; }
    public decimal Total { get; set; }

    public decimal DineroRecibido { get; set; }
    public decimal Cambio { get; set; }

    public MetodoPago MetodoPago { get; set; } = MetodoPago.Efectivo;
    public string? Nota { get; set; }
    public string? Nota1 { get; set; }
    public string? Nota2 { get; set; }

    public List<DetalleVenta> Detalles { get; set; } = new();
}