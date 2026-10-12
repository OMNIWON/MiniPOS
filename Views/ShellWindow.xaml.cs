using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using POS.ViewModels;

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

        var transferencia = App.AppHost.Services.GetRequiredService<CotizacionTransferService>().Consumir();
        if (transferencia is not null)
        {
            var venta = App.AppHost.Services.GetRequiredService<MainWindow>();
            venta.Owner = this;
            try
            {
                await venta.CargarCotizacionAsync(transferencia);
                venta.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"No se pudo cargar la cotización en ventas.\n\nDetalle: {ex.Message}", "Error al crear venta", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        await _viewModel.RefrescarAsync();
    }

    private void AbrirConfiguracion_Click(object sender, RoutedEventArgs e)
    {
        var ventana = App.AppHost.Services.GetRequiredService<ConfiguracionWindow>();
        ventana.Owner = this;
        ventana.ShowDialog();
    }
}