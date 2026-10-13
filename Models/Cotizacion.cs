namespace POS.Models;

public class Cotizacion
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string? Cliente { get; set; }
    public decimal Total { get; set; }
    public string? Nota1 { get; set; }
    public string? Nota2 { get; set; }

    public List<DetalleCotizacion> Detalles { get; set; } = new();
}