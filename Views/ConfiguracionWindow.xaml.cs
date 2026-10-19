using System.Windows;
using POS.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using POS.Services;

namespace POS.Views;

public partial class ConfiguracionWindow : Window
{
    public ConfiguracionWindow(ConfiguracionViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.Guardado += () => DialogResult = true;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
    private void ImprimirPrueba_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            App.AppHost.Services.GetRequiredService<ITicketService>().ImprimirPrueba();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo imprimir la prueba.\n\nDetalle: {ex.Message}",
                "Error de impresión", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}