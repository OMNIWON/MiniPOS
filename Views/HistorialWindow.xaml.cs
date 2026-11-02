using System.Windows;
using POS.ViewModels;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using POS.Models;
using POS.Services;
namespace POS.Views;

public partial class HistorialWindow : Window
{
    public HistorialWindow(HistorialViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.SolicitarCerrar += Close;
    }

    private void RegresarAlMenu_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
    private void CotizacionGuardarPdf_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).Tag is not Cotizacion cotizacion) return;

        var dialogo = new SaveFileDialog
        {
            Title = "Guardar cotización en PDF",
            Filter = "Documento PDF (*.pdf)|*.pdf",
            FileName = NombreArchivoCotizacion(cotizacion)
        };

        if (dialogo.ShowDialog(this) != true) return;

        try
        {
            App.AppHost.Services.GetRequiredService<ICotizacionPdfService>()
                .Generar(cotizacion, dialogo.FileName);

            var abrir = MessageBox.Show("El PDF se guardó correctamente.\n\n¿Quieres abrirlo ahora?",
                "PDF listo", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (abrir == MessageBoxResult.Yes)
                AbrirArchivo(dialogo.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo crear el PDF.\n\nDetalle: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CotizacionImprimir_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).Tag is not Cotizacion cotizacion) return;

        try
        {
            string ruta = Path.Combine(Path.GetTempPath(),
                $"Cotizacion-{cotizacion.Id}-{DateTime.Now:HHmmss}.pdf");

            App.AppHost.Services.GetRequiredService<ICotizacionPdfService>().Generar(cotizacion, ruta);
            AbrirArchivo(ruta);

            MessageBox.Show(
                "Se abrió la cotización en tu lector de PDF.\n\nPara imprimirla presiona Ctrl+P (o el icono de la impresora).",
                "Imprimir", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo preparar la cotización para imprimir.\n\nDetalle: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CotizacionCorreo_Click(object sender, RoutedEventArgs e)
    {
        if (((FrameworkElement)sender).Tag is not Cotizacion cotizacion) return;

        var ventana = ActivatorUtilities.CreateInstance<EnviarCorreoWindow>(App.AppHost.Services, cotizacion);
        ventana.Owner = this;
        ventana.ShowDialog();
    }

    private static void AbrirArchivo(string ruta)
    {
        Process.Start(new ProcessStartInfo(ruta) { UseShellExecute = true });
    }

    private static string NombreArchivoCotizacion(Cotizacion cotizacion)
    {
        string nombre = $"Cotizacion-{cotizacion.Id}";

        if (!string.IsNullOrWhiteSpace(cotizacion.Cliente))
        {
            var invalidos = Path.GetInvalidFileNameChars();
            string limpio = string.Concat(cotizacion.Cliente.Trim().Select(c => invalidos.Contains(c) ? '_' : c));
            nombre += $"-{limpio}";
        }

        return nombre + ".pdf";
    }
}