using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using POS.ViewModels;
using POS.Models;
using POS.Services;

namespace POS.Views;

public partial class ShellWindow : Window
{
    private readonly ShellViewModel _viewModel;

    public ShellWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private async void AbrirVenta_Click(object sender, RoutedEventArgs e)
    {
        var ventana = App.AppHost.Services.GetRequiredService<MainWindow>();
        ventana.Owner = this;
        ventana.ShowDialog();
        await _viewModel.RefrescarAsync();
    }

    private void AbrirCotizar_Click(object sender, RoutedEventArgs e)
    {
        var ventana = App.AppHost.Services.GetRequiredService<CotizarWindow>();
        ventana.Owner = this;
        ventana.ShowDialog();
    }

    private async void AbrirInventario_Click(object sender, RoutedEventArgs e)
    {
        var ventana = App.AppHost.Services.GetRequiredService<InventarioWindow>();
        ventana.Owner = this;
        ventana.ShowDialog();
        await _viewModel.RefrescarAsync();
    }

    private async void AbrirHistorial_Click(object sender, RoutedEventArgs e)
    {
        var ventana = App.AppHost.Services.GetRequiredService<HistorialWindow>();
        ventana.Owner = this;
        ventana.ShowDialog();

        var cotizacion = App.AppHost.Services.GetRequiredService<CotizacionTransferService>().Consumir();
        if (cotizacion is not null)
            await AbrirVentaDesdeCotizacionAsync(cotizacion);

        await _viewModel.RefrescarAsync();
    }

    private async Task AbrirVentaDesdeCotizacionAsync(Cotizacion cotizacion)
    {
        try
        {
            var venta = App.AppHost.Services.GetRequiredService<MainWindow>();
            venta.Owner = this;

            var omitidos = await venta.CargarCotizacionAsync(cotizacion);
            if (omitidos.Count > 0)
            {
                MessageBox.Show(
                    "Estos artículos ya no están disponibles y no se cargaron al carrito:\n\n• " +
                    string.Join("\n• ", omitidos),
                    "Algunos artículos no se cargaron", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            venta.ShowDialog();
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "Pasar cotización a venta");
            MessageBox.Show($"No se pudo cargar la cotización en una venta.\n\nDetalle: {ex.Message}",
                "Error al crear venta", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void AbrirConfiguracion_Click(object sender, RoutedEventArgs e)
    {
        var ventana = App.AppHost.Services.GetRequiredService<ConfiguracionWindow>();
        ventana.Owner = this;
        ventana.ShowDialog();
    }
}