using System.Windows;
using POS.Models;
using POS.Services;
using POS.ViewModels;
using POS.Views;

namespace POS;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly ITicketService _ticketService;

    public MainWindow(MainViewModel viewModel, ITicketService ticketService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _ticketService = ticketService;
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

        var cobroWindow = new CobroWindow(_viewModel.DesgloseActual) { Owner = this };
        if (cobroWindow.ShowDialog() != true) return;

        Venta venta;
        try
        {
            venta = await _viewModel.ConfirmarVentaAsync(
                cobroWindow.ViewModel.DineroRecibido,
                cobroWindow.ViewModel.MetodoSeleccionado);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"No se pudo completar la venta.\n\nLa información no se ha perdido. Intenta nuevamente.\n\nDetalle: {ex.Message}",
                "Error al guardar", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        string resumen = venta.MetodoPago == MetodoPago.Efectivo
            ? $"Total: {venta.Total:C2}\nCambio: {venta.Cambio:C2}"
            : $"Total: {venta.Total:C2}\nPagado con tarjeta";

        var imprimir = MessageBox.Show(
            $"Venta registrada correctamente.\n\n{resumen}\n\n¿Imprimir ticket?",
            "Venta confirmada", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (imprimir != MessageBoxResult.Yes) return;

        try
        {
            _ticketService.Imprimir(venta);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"La venta quedó guardada, pero no se pudo imprimir el ticket.\n\nDetalle: {ex.Message}",
                "Error de impresión", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}