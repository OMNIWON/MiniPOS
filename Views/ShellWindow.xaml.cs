using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace POS.Views;

public partial class ShellWindow : Window
{
    public ShellWindow()
    {
        InitializeComponent();
    }

    private void AbrirVenta_Click(object sender, RoutedEventArgs e)
    {
        var ventana = App.AppHost.Services.GetRequiredService<MainWindow>();
        ventana.Owner = this;
        ventana.ShowDialog();
    }

    private void AbrirCotizar_Click(object sender, RoutedEventArgs e)
    {
        var ventana = App.AppHost.Services.GetRequiredService<POS.Views.CotizarWindow>();
        ventana.Owner = this;
        ventana.ShowDialog();
    }

    private void AbrirInventario_Click(object sender, RoutedEventArgs e)
    {
        var ventana = App.AppHost.Services.GetRequiredService<POS.Views.InventarioWindow>();
        ventana.Owner = this;
        ventana.ShowDialog();
    }

    private void AbrirHistorial_Click(object sender, RoutedEventArgs e)
    {
        var ventana = App.AppHost.Services.GetRequiredService<POS.Views.HistorialWindow>();
        ventana.Owner = this;
        ventana.ShowDialog();
    }
}