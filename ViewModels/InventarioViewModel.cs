using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Models;
using POS.Services;
using POS.Views;

namespace POS.ViewModels;

public partial class InventarioViewModel : ObservableObject
{
    private readonly IProductoService _productoService;
    private readonly IInventarioService _inventarioService;

    public ObservableCollection<Producto> Productos { get; } = new();

    [ObservableProperty]
    private string textoBusqueda = string.Empty;

    public ProductoFormViewModel Formulario { get; }

    [ObservableProperty]
    private bool formularioVisible;

    public InventarioViewModel(IProductoService productoService, IInventarioService inventarioService, ProductoFormViewModel formulario)
    {
        _productoService = productoService;
        _inventarioService = inventarioService;
        Formulario = formulario;
        Formulario.GuardadoConExito += async () =>
        {
            FormularioVisible = false;
            await CargarAsync();
        };
        _ = CargarAsync();
    }

    private async Task CargarAsync()
    {
        var todos = await _productoService.ObtenerTodosAsync();
        var filtrados = string.IsNullOrWhiteSpace(TextoBusqueda)
            ? todos
            : todos.Where(p => p.Nombre.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase)
                             || p.Sku.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase));

        Productos.Clear();
        foreach (var p in filtrados)
            Productos.Add(p);
    }

    partial void OnTextoBusquedaChanged(string value) => _ = CargarAsync();

    [RelayCommand]
    private void NuevoProducto()
    {
        Formulario.CargarNuevo();
        FormularioVisible = true;
    }

    [RelayCommand]
    private void EditarProducto(Producto producto)
    {
        Formulario.CargarExistente(producto);
        FormularioVisible = true;
    }

    [RelayCommand]
    private void CerrarFormulario() => FormularioVisible = false;

    [RelayCommand]
    private async Task CambiarEstadoAsync(Producto producto)
    {
        if (producto.Activo)
            await _productoService.DesactivarAsync(producto.Id);
        else
            await _productoService.ActivarAsync(producto.Id);

        await CargarAsync();
    }

    [RelayCommand]
    private async Task EliminarProductoAsync(Producto producto)
    {
        var confirmacion = MessageBox.Show(
            $"¿Eliminar permanentemente \"{producto.Nombre}\"? Esta acción no se puede deshacer.",
            "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmacion != MessageBoxResult.Yes) return;

        bool eliminado = await _productoService.EliminarAsync(producto.Id);

        if (!eliminado)
        {
            MessageBox.Show(
                $"No se puede eliminar \"{producto.Nombre}\" porque ya tiene ventas o cotizaciones registradas. Puedes desactivarlo en su lugar.",
                "No se puede eliminar", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        await CargarAsync();
    }

    [RelayCommand]
    private async Task EntradaStockAsync(Producto producto)
    {
        decimal? cantidad = PedirCantidad(producto, "Entrada de mercancía",
            $"¿Cuántas unidades de \"{producto.Nombre}\" llegaron?");
        if (cantidad is null) return;

        await _inventarioService.AumentarStockAsync(producto.Id, cantidad.Value);
        await CargarAsync();
    }

    [RelayCommand]
    private async Task SalidaStockAsync(Producto producto)
    {
        decimal? cantidad = PedirCantidad(producto, "Salida de mercancía",
            $"¿Cuántas unidades de \"{producto.Nombre}\" quieres dar de baja? (merma, rotura, consumo propio)");
        if (cantidad is null) return;

        if (cantidad.Value > producto.Existencia)
        {
            MessageBox.Show(
                $"Solo hay {producto.Existencia} en existencia de \"{producto.Nombre}\". No se puede sacar más de lo que hay.",
                "Cantidad mayor a la existencia", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        await _inventarioService.DisminuirStockAsync(producto.Id, cantidad.Value);
        await CargarAsync();
    }

    private static decimal? PedirCantidad(Producto producto, string titulo, string pregunta)
    {
        var ventana = new CantidadWindow(titulo, pregunta, producto.PermiteFraccion);

        var activa = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
        if (activa is not null)
            ventana.Owner = activa;

        return ventana.ShowDialog() == true ? ventana.Cantidad : null;
    }
}