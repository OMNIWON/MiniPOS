namespace POS.Services;

public interface IInventarioService
{
    Task AumentarStockAsync(int productoId, decimal cantidad);
    Task<bool> DisminuirStockAsync(int productoId, decimal cantidad);
    Task<bool> HayStockSuficienteAsync(int productoId, decimal cantidad);
    Task<List<POS.Models.Producto>> ObtenerBajoStockMinimoAsync();
}