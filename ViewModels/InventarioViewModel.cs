using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Models;
using POS.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace POS.ViewModels;

public partial class InventarioViewModel : ObservableObject
{
    private readonly IProductoService _productoService;

    public ObservableCollection<Producto> Productos { get; } = new();

    [ObservableProperty]
    private string textoBusqueda = string.Empty;

    public ProductoFormViewModel Formulario { get; }

    [ObservableProperty]
    private bool formularioVisible;

    public InventarioViewModel(IProductoService productoService, ProductoFormViewModel formulario)
    {
        _productoService = productoService;
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
    private void CerrarFormulario()
    {
        FormularioVisible = false;
    }

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
}