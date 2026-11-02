namespace POS.Models;

public class ClienteGuardado
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}

public class CorreoGuardado
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
}