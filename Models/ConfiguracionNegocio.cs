namespace POS.Models;

public class ConfiguracionNegocio
{
    public int Id { get; set; }
    public string NombreNegocio { get; set; } = "Mi Negocio";
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? MensajeTicket { get; set; } = "¡Gracias por su compra!";
    public int AnchoTicketMm { get; set; } = 80;
    public bool IvaActivo { get; set; }
    public decimal TasaIva { get; set; } = 16m;
    public bool PreciosIncluyenIva { get; set; } = true;
}