using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public partial class CotizarViewModel : ObservableObject
{
    private readonly IProductoService _productoService;
    private readonly ICotizacionService _cotizacionService;

    public ObservableCollection<Producto> ProductosEncontrados { get; } = new();
    public ObservableCollection<CarritoItemViewModel> Carrito { get; } = new();

    [ObservableProperty]
    private string textoBusqueda = string.Empty;

    [ObservableProperty]
    private string? cliente;

    [ObservableProperty]
    private decimal total;

    [ObservableProperty]
    private string? mensajeInfo;

    public CotizarViewModel(IProductoService productoService, ICotizacionService cotizacionService)
    {
        _productoService = productoService;
        _cotizacionService = cotizacionService;
        Carrito.CollectionChanged += Carrito_CollectionChanged;
        _ = CargarProductosAsync();
    }

    private void Carrito_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
            foreach (CarritoItemViewModel item in e.NewItems)
                item.PropertyChanged += CarritoItem_PropertyChanged;

        if (e.OldItems is not null)
            foreach (CarritoItemViewModel item in e.OldItems)
                item.PropertyChanged -= CarritoItem_PropertyChanged;

        RecalcularTotal();
    }

    private void CarritoItem_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CarritoItemViewModel.Subtotal))
            RecalcularTotal();
    }

    private async Task CargarProductosAsync()
    {
        try
        {
            var productos = string.IsNullOrWhiteSpace(TextoBusqueda)
                ? await _productoService.ObtenerActivosAsync()
                : await _productoService.BuscarAsync(TextoBusqueda);

            ProductosEncontrados.Clear();
            foreach (var producto in productos)
                ProductosEncontrados.Add(producto);
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "Carga de productos para cotización");
            MensajeInfo = "No se pudieron cargar los productos. Verifica la conexión con la base de datos.";
        }
    }

    partial void OnTextoBusquedaChanged(string value)
    {
        _ = CargarProductosAsync();
    }

    [RelayCommand]
    private void AgregarAlCarrito(Producto producto)
    {
        var existente = Carrito.FirstOrDefault(c => c.Producto.Id == producto.Id);
        if (existente is not null)
            existente.Cantidad += 1m;
        else
            Carrito.Add(new CarritoItemViewModel(producto, 1m));
    }

    [RelayCommand]
    private void IncrementarCantidad(CarritoItemViewModel item) => CambiarCantidad(item, item.Cantidad + 1m);

    [RelayCommand]
    private void DecrementarCantidad(CarritoItemViewModel item) => CambiarCantidad(item, item.Cantidad - 1m);

    [RelayCommand]
    private void QuitarDelCarrito(CarritoItemViewModel item) => Carrito.Remove(item);

    [RelayCommand]
    private void VaciarCarrito()
    {
        Carrito.Clear();
        Cliente = null;
        MensajeInfo = null;
    }

    public void CambiarCantidad(CarritoItemViewModel item, decimal nuevaCantidad)
    {
        if (nuevaCantidad <= 0)
        {
            QuitarDelCarrito(item);
            return;
        }

        if (!item.Producto.PermiteFraccion)
            nuevaCantidad = Math.Floor(nuevaCantidad);

        item.Cantidad = nuevaCantidad <= 0 ? 1 : nuevaCantidad;
    }

    [RelayCommand]
    private async Task GuardarCotizacionAsync()
    {
        if (Carrito.Count == 0)
        {
            MensajeInfo = "Agrega al menos un producto antes de guardar la cotización.";
            return;
        }

        var detalles = Carrito.Select(c => new DetalleCotizacion
        {
            ProductoId = c.Producto.Id,
            NombreProducto = c.Producto.Nombre,
            Cantidad = c.Cantidad,
            PrecioUnitario = c.PrecioUnitario,
            DescuentoPorcentaje = c.DescuentoPorcentaje,
            Subtotal = c.Subtotal
        }).ToList();

        await _cotizacionService.GuardarAsync(detalles, Cliente);

        Carrito.Clear();
        Cliente = null;
        MensajeInfo = "Cotización guardada correctamente.";
    }

    private void RecalcularTotal()
    {
        Total = Carrito.Sum(c => c.Subtotal);
    }
}