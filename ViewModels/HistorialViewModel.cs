using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Models;
using POS.Services;
using POS.Views;

namespace POS.ViewModels;

public partial class HistorialViewModel : ObservableObject
{
    private readonly IVentaService _ventaService;
    private readonly ICotizacionService _cotizacionService;
    private readonly ITicketService _ticketService;
    private readonly CotizacionTransferService _cotizacionTransfer;

    public event Action? SolicitarCerrar;

    public ObservableCollection<Venta> Ventas { get; } = new();
    public ObservableCollection<Cotizacion> Cotizaciones { get; } = new();

    [ObservableProperty]
    private bool mostrandoVentas = true;

    [ObservableProperty]
    private Venta? ventaSeleccionada;

    [ObservableProperty]
    private Cotizacion? cotizacionSeleccionada;

    public HistorialViewModel(IVentaService ventaService, ICotizacionService cotizacionService, ITicketService ticketService, CotizacionTransferService cotizacionTransfer)
    {
        _ventaService = ventaService;
        _cotizacionService = cotizacionService;
        _ticketService = ticketService;
        _cotizacionTransfer = cotizacionTransfer;
        _ = CargarAsync();
    }

    private async Task CargarAsync()
    {
        try
        {
            var ventas = await _ventaService.ObtenerHistorialAsync();
            var cotizaciones = await _cotizacionService.ObtenerHistorialAsync();

            Ventas.Clear();
            foreach (var venta in ventas)
                Ventas.Add(venta);

            Cotizaciones.Clear();
            foreach (var cotizacion in cotizaciones)
                Cotizaciones.Add(cotizacion);
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "Carga del historial");
            MessageBox.Show("No se pudo cargar el historial. Revisa la base de datos e inténtalo nuevamente.\n\n" + ex.Message,
                "Error al cargar historial", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void VerVentas()
    {
        MostrandoVentas = true;
        VentaSeleccionada = null;
        CotizacionSeleccionada = null;
    }

    [RelayCommand]
    private void VerCotizaciones()
    {
        MostrandoVentas = false;
        VentaSeleccionada = null;
        CotizacionSeleccionada = null;
    }

    [RelayCommand]
    private void SeleccionarVenta(Venta venta) => VentaSeleccionada = venta;

    [RelayCommand]
    private void SeleccionarCotizacion(Cotizacion cotizacion) => CotizacionSeleccionada = cotizacion;

    [RelayCommand]
    private async Task EditarNotasVentaAsync(Venta venta)
    {
        var ventana = new NotasWindow(venta.Nota1, venta.Nota2);
        if (ventana.ShowDialog() != true) return;

        try
        {
            await _ventaService.GuardarNotasAsync(venta.Id, ventana.Nota1, ventana.Nota2);
            venta.Nota1 = ventana.Nota1;
            venta.Nota2 = ventana.Nota2;
            OnPropertyChanged(nameof(Ventas));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudieron guardar las notas.\n\nDetalle: {ex.Message}", "Error al guardar notas", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task EditarNotasCotizacionAsync(Cotizacion cotizacion)
    {
        var ventana = new NotasWindow(cotizacion.Nota1, cotizacion.Nota2);
        if (ventana.ShowDialog() != true) return;

        try
        {
            await _cotizacionService.GuardarNotasAsync(cotizacion.Id, ventana.Nota1, ventana.Nota2);
            cotizacion.Nota1 = ventana.Nota1;
            cotizacion.Nota2 = ventana.Nota2;
            OnPropertyChanged(nameof(Cotizaciones));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudieron guardar las notas.\n\nDetalle: {ex.Message}", "Error al guardar notas", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private void PasarAVenta(Cotizacion cotizacion)
    {
        if (cotizacion.Detalles.Count == 0)
        {
            MessageBox.Show("Esta cotización no contiene artículos.", "Cotización vacía", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirmar = MessageBox.Show($"¿Cargar los {cotizacion.Detalles.Count} artículos de la cotización de {cotizacion.Fecha:dd/MM/yyyy HH:mm} al carrito de venta?\n\nLa cotización original se conservará.", "Crear venta desde cotización", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (confirmar != MessageBoxResult.Yes) return;

        _cotizacionTransfer.Preparar(cotizacion);
        SolicitarCerrar?.Invoke();
    }

    [RelayCommand]
    private void ImprimirTicket(Venta venta)
    {
        try
        {
            _ticketService.Imprimir(venta);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo imprimir el ticket.\n\nDetalle: {ex.Message}",
                "Error de impresión", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task AnularVentaAsync(Venta venta)
    {
        var confirmacion = MessageBox.Show(
            $"¿Anular esta venta del {venta.Fecha:dd/MM/yyyy HH:mm} por {venta.Total:C2}?\n\n" +
            "El inventario de los productos vendidos se devolverá automáticamente.",
            "Confirmar anulación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmacion != MessageBoxResult.Yes) return;

        try
        {
            await _ventaService.AnularVentaAsync(venta.Id);
            VentaSeleccionada = null;
            await CargarAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo anular la venta.\n\nDetalle: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task EliminarCotizacionAsync(Cotizacion cotizacion)
    {
        var confirmacion = MessageBox.Show(
            $"¿Eliminar esta cotización del {cotizacion.Fecha:dd/MM/yyyy HH:mm}?",
            "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirmacion != MessageBoxResult.Yes) return;

        await _cotizacionService.EliminarAsync(cotizacion.Id);
        CotizacionSeleccionada = null;
        await CargarAsync();
    }
}