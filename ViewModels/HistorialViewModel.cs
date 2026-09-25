using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public partial class HistorialViewModel : ObservableObject
{
    private readonly IVentaService _ventaService;
    private readonly ICotizacionService _cotizacionService;

    public ObservableCollection<Venta> Ventas { get; } = new();
    public ObservableCollection<Cotizacion> Cotizaciones { get; } = new();

    [ObservableProperty]
    private bool mostrandoVentas = true;

    [ObservableProperty]
    private Venta? ventaSeleccionada;

    [ObservableProperty]
    private Cotizacion? cotizacionSeleccionada;

    public HistorialViewModel(IVentaService ventaService, ICotizacionService cotizacionService)
    {
        _ventaService = ventaService;
        _cotizacionService = cotizacionService;
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
    private void SeleccionarVenta(Venta venta)
    {
        VentaSeleccionada = venta;
    }

    [RelayCommand]
    private void SeleccionarCotizacion(Cotizacion cotizacion)
    {
        CotizacionSeleccionada = cotizacion;
    }
}