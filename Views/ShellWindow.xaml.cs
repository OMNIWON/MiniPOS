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
        await _viewModel.RefrescarAsync();
    }

    private void AbrirConfiguracion_Click(object sender, RoutedEventArgs e)
    {
        var ventana = App.AppHost.Services.GetRequiredService<ConfiguracionWindow>();
        ventana.Owner = this;
        ventana.ShowDialog();
    }
}