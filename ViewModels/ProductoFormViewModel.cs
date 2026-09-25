using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public partial class ProductoFormViewModel : ObservableObject
{
    private readonly IProductoService _productoService;
    private int _productoId;

    public ObservableCollection<Categoria> Categorias { get; } = new();
    public Array UnidadesDisponibles => Enum.GetValues(typeof(UnidadMedida));

    [ObservableProperty] private string sku = string.Empty;
    [ObservableProperty] private string nombre = string.Empty;
    [ObservableProperty] private string? descripcion;
    [ObservableProperty] private Categoria? categoriaSeleccionada;
    [ObservableProperty] private UnidadMedida unidadSeleccionada;
    [ObservableProperty] private decimal precioCompra;
    [ObservableProperty] private decimal precioVenta;
    [ObservableProperty] private decimal existencia;
    [ObservableProperty] private decimal stockMinimo;
    [ObservableProperty] private string? rutaImagen;
    [ObservableProperty] private string? mensaje;
    [ObservableProperty] private bool esNuevo = true;

    public event Action? GuardadoConExito;

    public ProductoFormViewModel(IProductoService productoService)
    {
        _productoService = productoService;
        _ = CargarCategoriasAsync();
    }

    private async Task CargarCategoriasAsync()
    {
        Categorias.Clear();
        foreach (var c in await _productoService.ObtenerCategoriasAsync())
            Categorias.Add(c);
    }

    public void CargarNuevo()
    {
        EsNuevo = true;
        _productoId = 0;
        Sku = string.Empty;
        Nombre = string.Empty;
        Descripcion = null;
        CategoriaSeleccionada = Categorias.FirstOrDefault();
        UnidadSeleccionada = UnidadMedida.Pieza;
        PrecioCompra = 0;
        PrecioVenta = 0;
        Existencia = 0;
        StockMinimo = 0;
        RutaImagen = null;
        Mensaje = null;
    }

    public void CargarExistente(Producto producto)
    {
        EsNuevo = false;
        _productoId = producto.Id;
        Sku = producto.Sku;
        Nombre = producto.Nombre;
        Descripcion = producto.Descripcion;
        CategoriaSeleccionada = Categorias.FirstOrDefault(c => c.Id == producto.CategoriaId);
        UnidadSeleccionada = producto.Unidad;
        PrecioCompra = producto.PrecioCompra;
        PrecioVenta = producto.PrecioVenta;
        Existencia = producto.Existencia;
        StockMinimo = producto.StockMinimo;
        RutaImagen = producto.RutaImagen;
        Mensaje = null;
    }

    [RelayCommand]
    private void ElegirImagen()
    {
        var dialogo = new OpenFileDialog
        {
            Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp",
            Title = "Selecciona una foto del producto"
        };

        if (dialogo.ShowDialog() == true)
        {
            RutaImagen = dialogo.FileName;
        }
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nombre))
        {
            Mensaje = "El nombre es obligatorio.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Sku))
        {
            Sku = await _productoService.GenerarSkuAsync(Nombre.Trim());
        }
        else if (await _productoService.ExisteSkuAsync(Sku.Trim(), _productoId))
        {
            Mensaje = $"Ya existe otro producto con el SKU \"{Sku.Trim()}\". Usa un SKU distinto.";
            return;
        }

        if (CategoriaSeleccionada is null)
        {
            Mensaje = "Selecciona una categoría.";
            return;
        }

        if (PrecioVenta < 0 || PrecioCompra < 0)
        {
            Mensaje = "Los precios no pueden ser negativos.";
            return;
        }

        if (Existencia < 0 || StockMinimo < 0)
        {
            Mensaje = "La existencia y el stock mínimo no pueden ser negativos.";
            return;
        }

        var producto = new Producto
        {
            Id = _productoId,
            Sku = Sku.Trim(),
            Nombre = Nombre.Trim(),
            Descripcion = Descripcion,
            CategoriaId = CategoriaSeleccionada.Id,
            Unidad = UnidadSeleccionada,
            PrecioCompra = PrecioCompra,
            PrecioVenta = PrecioVenta,
            Existencia = Existencia,
            StockMinimo = StockMinimo,
            RutaImagen = RutaImagen,
            Activo = true
        };

        if (EsNuevo)
        {
            await _productoService.CrearAsync(producto);
            if (!string.IsNullOrWhiteSpace(RutaImagen) && File.Exists(RutaImagen))
            {
                string rutaFinal = _productoService.GuardarImagen(RutaImagen, producto.Id);
                producto.RutaImagen = rutaFinal;
                await _productoService.EditarAsync(producto);
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(RutaImagen) && File.Exists(RutaImagen) && !RutaImagen.Contains("Imagenes"))
            {
                producto.RutaImagen = _productoService.GuardarImagen(RutaImagen, producto.Id);
            }
            await _productoService.EditarAsync(producto);
        }

        Mensaje = "Producto guardado correctamente.";
        GuardadoConExito?.Invoke();
    }
}