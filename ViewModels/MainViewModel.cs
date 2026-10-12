using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IProductoService _productoService;
    private readonly IVentaService _ventaService;
    private readonly IConfiguracionService _configuracion;

    public ObservableCollection<Producto> ProductosEncontrados { get; } = new();
    public ObservableCollection<CarritoItemViewModel> Carrito { get; } = new();

    [ObservableProperty] private string textoBusqueda = string.Empty;
    [ObservableProperty] private decimal subtotal;
    [ObservableProperty] private decimal impuestos;
    [ObservableProperty] private decimal total;
    [ObservableProperty] private string? mensajeError;

    public bool MuestraIva => _configuracion.Actual.IvaActivo;
    public string EtiquetaIva => $"IVA ({_configuracion.Actual.TasaIva:0.##}%)";
    public DesgloseVenta DesgloseActual { get; private set; } = new(0m, 0m, 0m, 0m);

    public MainViewModel(IProductoService productoService, IVentaService ventaService, IConfiguracionService configuracion)
    {
        _productoService = productoService;
        _ventaService = ventaService;
        _configuracion = configuracion;
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
            ErrorLogger.Log(ex, "Carga de productos para venta");
            MensajeError = "No se pudieron cargar los productos. Verifica la conexión con la base de datos.";
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
        {
            existente.Cantidad += 1m;
            AvisarSiFaltaStock(existente);
        }
        else
        {
            var item = new CarritoItemViewModel(producto, 1m);
            Carrito.Add(item);
            AvisarSiFaltaStock(item);
        }
    }

    [RelayCommand]
    private void IncrementarCantidad(CarritoItemViewModel item) => CambiarCantidad(item, item.Cantidad + 1m);

    [RelayCommand]
    private void DecrementarCantidad(CarritoItemViewModel item) => CambiarCantidad(item, item.Cantidad - 1m);

    [RelayCommand]
    private void QuitarDelCarrito(CarritoItemViewModel item)
    {
        Carrito.Remove(item);
        MensajeError = null;
    }

    [RelayCommand]
    private void VaciarCarrito()
    {
        Carrito.Clear();
        MensajeError = null;
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

        if (nuevaCantidad <= 0)
        {
            QuitarDelCarrito(item);
            return;
        }

        item.Cantidad = nuevaCantidad;
        AvisarSiFaltaStock(item);
    }

    private void AvisarSiFaltaStock(CarritoItemViewModel item)
    {
        MensajeError = item.Cantidad > item.Producto.Existencia
            ? $"Aviso: solo hay {item.Producto.Existencia} {UnidadTexto(item.Producto)} de {item.Producto.Nombre} en inventario. Se venderá bajo pedido."
            : null;
    }

    private void RecalcularTotal()
    {
        decimal suma = Carrito.Sum(c => c.Subtotal);
        DesgloseActual = CalculoImpuestos.Calcular(suma, _configuracion.Actual);

        Subtotal = DesgloseActual.Subtotal;
        Impuestos = DesgloseActual.Impuestos;
        Total = DesgloseActual.Total;
    }

    public async Task CargarCotizacionAsync(Cotizacion cotizacion)
    {
        if (cotizacion.Detalles.Count == 0)
            throw new InvalidOperationException("La cotización no contiene artículos.");

        Carrito.Clear();
        foreach (var detalle in cotizacion.Detalles)
        {
            var producto = await _productoService.ObtenerPorIdAsync(detalle.ProductoId);
            if (producto is null)
                throw new InvalidOperationException($"No se encontró el producto '{detalle.NombreProducto}' de la cotización.");

            var item = new CarritoItemViewModel(producto, detalle.Cantidad)
            {
                PrecioUnitario = detalle.PrecioUnitario,
                DescuentoPorcentaje = detalle.DescuentoPorcentaje
            };
            Carrito.Add(item);
            AvisarSiFaltaStock(item);
        }

        RecalcularTotal();
    }

    public async Task CargarCotizacionAsync(Cotizacion cotizacion)
    {
        if (cotizacion.Detalles.Count == 0)
            throw new InvalidOperationException("La cotización no contiene artículos.");

        Carrito.Clear();
        foreach (var detalle in cotizacion.Detalles)
        {
            var producto = await _productoService.ObtenerPorIdAsync(detalle.ProductoId);
            if (producto is null)
                throw new InvalidOperationException($"No se encontró el producto '{detalle.NombreProducto}' de la cotización.");

            var item = new CarritoItemViewModel(producto, detalle.Cantidad)
            {
                PrecioUnitario = detalle.PrecioUnitario,
                DescuentoPorcentaje = detalle.DescuentoPorcentaje
            };
            Carrito.Add(item);
            AvisarSiFaltaStock(item);
        }

        RecalcularTotal();
    }

    public async Task<Venta> ConfirmarVentaAsync(decimal dineroRecibido, MetodoPago metodoPago)
    {
        var detalles = Carrito.Select(c => new DetalleVenta
        {
            ProductoId = c.Producto.Id,
            NombreProducto = c.Producto.Nombre,
            Cantidad = c.Cantidad,
            PrecioUnitario = c.PrecioUnitario,
            DescuentoPorcentaje = c.DescuentoPorcentaje,
            Subtotal = c.Subtotal
        }).ToList();

        var venta = await _ventaService.ConfirmarVentaAsync(detalles, dineroRecibido, metodoPago, DesgloseActual);

        Carrito.Clear();
        MensajeError = null;
        await CargarProductosAsync();

        return venta;
    }

    private static string UnidadTexto(Producto producto) => producto.Unidad switch
    {
        UnidadMedida.Pieza => "piezas",
        UnidadMedida.Metro => "metros",
        UnidadMedida.Kilogramo => "kilogramos",
        UnidadMedida.Litro => "litros",
        _ => "unidades"
    };
}