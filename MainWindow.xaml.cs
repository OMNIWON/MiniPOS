using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using POS.ViewModels;
using POS.Views;

namespace POS;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;
    }

    private void RegresarAlMenu_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void Cobrar_Click(object sender, RoutedEventArgs e)
    {
        if (_viewModel.Carrito.Count == 0)
        {
            MessageBox.Show("Agrega al menos un producto antes de cobrar.", "Carrito vacío",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var cobroWindow = new CobroWindow(_viewModel.Total) { Owner = this };
        bool? resultado = cobroWindow.ShowDialog();

        if (resultado != true) return;

        try
        {
            var venta = await _viewModel.ConfirmarVentaAsync(cobroWindow.ViewModel.DineroRecibido);

            MessageBox.Show(
                $"Venta registrada correctamente.\n\nTotal: {venta.Total:C2}\nCambio: {venta.Cambio:C2}",
                "Venta confirmada", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo completar la venta.\n\nLa información no se ha perdido. Intenta nuevamente.\n\nDetalle: {ex.Message}",
                "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}