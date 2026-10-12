using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public partial class HistorialViewModel : ObservableObject
{
    private readonly IVentaService _ventaService;
    private readonly ICotizacionService _cotizacionService;
    private readonly ITicketService _ticketService;

    public ObservableCollection<Venta> Ventas { get; } = new();
    public ObservableCollection<Cotizacion> Cotizaciones { get; } = new();

    [ObservableProperty]
    private bool mostrandoVentas = true;

    [ObservableProperty]
    private Venta? ventaSeleccionada;

    [ObservableProperty]
    private Cotizacion? cotizacionSeleccionada;

    public HistorialViewModel(IVentaService ventaService, ICotizacionService cotizacionService, ITicketService ticketService)
    {
        _ventaService = ventaService;
        _cotizacionService = cotizacionService;
        _ticketService = ticketService;
        _ = CargarAsync();
    }

    private async Task CargarAsync()
    {
        Ventas.Clear();
        foreach (var v in await _ventaService.ObtenerHistorialAsync())
            Ventas.Add(v);

        Cotizaciones.Clear();
        foreach (var c in await _cotizacionService.ObtenerHistorialAsync())
            Cotizaciones.Add(c);
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