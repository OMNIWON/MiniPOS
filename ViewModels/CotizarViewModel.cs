using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public partial class CotizarViewModel : ObservableObject
{
    private readonly IProductoService _productoService;
    private readonly ICotizacionService _cotizacionService;
    private readonly IAgendaService _agenda;

    public ObservableCollection<Producto> ProductosEncontrados { get; } = new();
    public ObservableCollection<CarritoItemViewModel> Carrito { get; } = new();
    public ObservableCollection<string> ClientesSugeridos { get; } = new();

    [ObservableProperty] private string textoBusqueda = string.Empty;
    [ObservableProperty] private string? cliente;
    [ObservableProperty] private decimal total;
    [ObservableProperty] private string? mensajeInfo;
    [ObservableProperty] private bool mostrarOpciones;

    public CotizarViewModel(IProductoService productoService, ICotizacionService cotizacionService, IAgendaService agenda)
    {
        _productoService = productoService;
        _cotizacionService = cotizacionService;
        _agenda = agenda;
        Carrito.CollectionChanged += Carrito_CollectionChanged;
        _ = InicializarAsync();
    }

    // Se cargan en secuencia: todas las pantallas comparten una sola conexión a la base de datos.
    private async Task InicializarAsync()
    {
        await CargarProductosAsync();
        await CargarClientesAsync();
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
            foreach (var p in productos)
                ProductosEncontrados.Add(p);
        }
        catch (Exception ex)
        {
            MensajeInfo = $"No se pudieron cargar los productos: {ex.Message}";
        }
    }

    private async Task CargarClientesAsync()
    {
        try
        {
            var nombres = await _agenda.ObtenerClientesAsync();
            ClientesSugeridos.Clear();
            foreach (var nombre in nombres)
                ClientesSugeridos.Add(nombre);
        }
        catch
        {
            // Sin sugerencias; el campo cliente sigue funcionando.
        }
    }

    partial void OnTextoBusquedaChanged(string value)
    {
        _ = CargarProductosAsync();
    }

    [RelayCommand]
    private void AlternarOpciones() => MostrarOpciones = !MostrarOpciones;

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

        string? nombreCliente = string.IsNullOrWhiteSpace(Cliente) ? null : Cliente.Trim();

        var detalles = Carrito.Select(c => new DetalleCotizacion
        {
            ProductoId = c.Producto.Id,
            NombreProducto = c.Producto.Nombre,
            Cantidad = c.Cantidad,
            PrecioUnitario = c.PrecioUnitario,
            DescuentoPorcentaje = c.DescuentoPorcentaje,
            Subtotal = c.Subtotal
        }).ToList();

        await _cotizacionService.GuardarAsync(detalles, nombreCliente);

        Carrito.Clear();
        Cliente = null;
        MensajeInfo = "Cotización guardada correctamente.";

        if (nombreCliente is not null)
            await OfrecerGuardarClienteAsync(nombreCliente);
    }

    private async Task OfrecerGuardarClienteAsync(string nombre)
    {
        try
        {
            if (await _agenda.ExisteClienteAsync(nombre)) return;

            var respuesta = MessageBox.Show(
                $"¿Deseas guardar este cliente en tu agenda?\n\n{nombre}",
                "Guardar cliente", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (respuesta != MessageBoxResult.Yes) return;

            await _agenda.GuardarClienteAsync(nombre);
            await CargarClientesAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo guardar el cliente.\n\nDetalle: {ex.Message}",
                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void RecalcularTotal()
    {
        Total = Carrito.Sum(c => c.Subtotal);
    }
}