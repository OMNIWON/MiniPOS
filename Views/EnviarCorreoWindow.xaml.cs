using System.IO;
using System.Net.Mail;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using POS.Models;
using POS.Services;

namespace POS.Views;

public partial class EnviarCorreoWindow : Window
{
    private readonly Cotizacion _cotizacion;
    private readonly ICotizacionPdfService _pdf;
    private readonly ICorreoService _correo;

    public EnviarCorreoWindow(Cotizacion cotizacion, ICotizacionPdfService pdf,
        ICorreoService correo, IConfiguracionService configuracion)
    {
        InitializeComponent();
        _cotizacion = cotizacion;
        _pdf = pdf;
        _correo = correo;

        var negocio = configuracion.Actual;

        TextoTitulo.Text = $"Enviar cotización No. {cotizacion.Id}";
        CajaAsunto.Text = $"Cotización No. {cotizacion.Id} - {negocio.NombreNegocio}";

        string saludo = string.IsNullOrWhiteSpace(cotizacion.Cliente) ? "Hola" : $"Hola {cotizacion.Cliente}";
        string firma = negocio.NombreNegocio +
                       (string.IsNullOrWhiteSpace(negocio.Telefono) ? string.Empty : $"\nTel. {negocio.Telefono}");

        CajaMensaje.Text = $"{saludo},\n\nAdjuntamos la cotización No. {cotizacion.Id} que nos solicitó.\n\n" +
                           $"Quedamos atentos a cualquier duda.\n\n{firma}";

        TextoAdjunto.Text = $"📎 Se adjuntará: {NombreAdjunto()}";
    }

    private string NombreAdjunto() => $"Cotizacion-{_cotizacion.Id}.pdf";

    private async void Enviar_Click(object sender, RoutedEventArgs e)
    {
        string para = CajaPara.Text.Trim();

        if (!MailAddress.TryCreate(para, out _) || !para.Contains('@'))
        {
            Mostrar("Escribe un correo válido para el cliente.");
            return;
        }

        if (!_correo.Cargar().Configurado)
        {
            var config = App.AppHost.Services.GetRequiredService<CorreoConfigWindow>();
            config.Owner = this;
            if (config.ShowDialog() != true) return;
        }

        string ruta = Path.Combine(Path.GetTempPath(), NombreAdjunto());

        BotonEnviar.IsEnabled = false;
        Mostrar("Enviando...");

        try
        {
            await Task.Run(() => _pdf.Generar(_cotizacion, ruta));
            await _correo.EnviarAsync(para, CajaAsunto.Text.Trim(), CajaMensaje.Text, ruta);

            MessageBox.Show($"La cotización se envió a {para}.", "Enviado",
                MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
        }
        catch (Exception ex)
        {
            Mostrar(CorreoService.Explicar(ex));
            BotonEnviar.IsEnabled = true;
        }
        finally
        {
            try
            {
                if (File.Exists(ruta)) File.Delete(ruta);
            }
            catch
            {
                // El archivo temporal se limpia solo la próxima vez.
            }
        }
    }

    private void Configurar_Click(object sender, RoutedEventArgs e)
    {
        var config = App.AppHost.Services.GetRequiredService<CorreoConfigWindow>();
        config.Owner = this;
        config.ShowDialog();
    }

    private void Mostrar(string mensaje)
    {
        TextoMensaje.Text = mensaje;
        TextoMensaje.Visibility = Visibility.Visible;
    }
}