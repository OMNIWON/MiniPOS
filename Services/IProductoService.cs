using POS.Models;

namespace POS.Services;

public interface IProductoService
{
    Task<List<Producto>> ObtenerActivosAsync();
    Task<List<Producto>> ObtenerTodosAsync();
    Task<List<Producto>> BuscarAsync(string texto);
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task<List<Categoria>> ObtenerCategoriasAsync();
    Task CrearAsync(Producto producto);
    Task EditarAsync(Producto producto);
    Task DesactivarAsync(int id);
    Task ActivarAsync(int id);
    string GuardarImagen(string rutaOrigen, int productoId);

    Task<bool> EliminarAsync(int id);
    Task<bool> ExisteSkuAsync(string sku, int idExcluir);
    Task<string> GenerarSkuAsync(string nombre);
    Task<int> ContarStockBajoAsync();
}