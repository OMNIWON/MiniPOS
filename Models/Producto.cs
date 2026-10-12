namespace POS.Models;

public class Producto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }

    public UnidadMedida Unidad { get; set; } = UnidadMedida.Pieza;

    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }

    public decimal Existencia { get; set; }
    public decimal StockMinimo { get; set; }

    public string? RutaImagen { get; set; }

    public bool Activo { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime FechaModificacion { get; set; } = DateTime.Now;

    public bool PermiteFraccion => Unidad != UnidadMedida.Pieza;
    public bool EnStockBajo => Existencia <= StockMinimo;
}