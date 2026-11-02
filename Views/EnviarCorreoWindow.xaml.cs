using System.IO;
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
    private readonly IAgendaService _agenda;

    public EnviarCorreoWindow(Cotizacion cotizacion, ICotizacionPdfService pdf, ICorreoService correo,
        IConfiguracionService configuracion, IAgendaService agenda)
    {
        InitializeComponent();
        _cotizacion = cotizacion;
        _pdf = pdf;
        _correo = correo;
        _agenda = agenda;

        var negocio = configuracion.Actual;

        TextoTitulo.Text = $"Enviar cotización No. {cotizacion.Id}";
        CajaAsunto.Text = $"Cotización No. {cotizacion.Id} - {negocio.NombreNegocio}";

        string saludo = string.IsNullOrWhiteSpace(cotizacion.Cliente) ? "Hola" : $"Hola {cotizacion.Cliente}";
        string firma = negocio.NombreNegocio +
                       (string.IsNullOrWhiteSpace(negocio.Telefono) ? string.Empty : $"\nTel. {negocio.Telefono}");

        CajaMensaje.Text = $"{saludo},\n\nAdjuntamos la cotización No. {cotizacion.Id} que nos solicitó.\n\n" +
                           $"Quedamos atentos a cualquier duda.\n\n{firma}";

        TextoAdjunto.Text = $"📎 Se adjuntará: {NombreAdjunto()}";

        Loaded += async (s, e) =>
        {
            try
            {
                CajaPara.ItemsSource = await _agenda.ObtenerCorreosAsync();
            }
            catch
            {
                // Sin sugerencias; el campo sigue funcionando.
            }
        };
    }

    private string NombreAdjunto() => $"Cotizacion-{_cotizacion.Id}.pdf";

    private async void Enviar_Click(object sender, RoutedEventArgs e)
    {
        string para = CajaPara.Text.Trim();

        var formato = ValidadorCorreo.Validar(para);
        if (!formato.EsValido)
        {
            if (formato.Sugerencia is null)
            {
                Mostrar(formato.Mensaje ?? "El correo no es válido.");
                return;
            }

            var respuesta = MessageBox.Show(
                $"{formato.Mensaje}\n\n¿Quisiste decir {formato.Sugerencia}?",
                "Revisa el correo", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (respuesta == MessageBoxResult.Yes)
            {
                CajaPara.Text = formato.Sugerencia;
                Mostrar("Corregimos el correo. Revísalo y presiona Enviar.");
            }

            return;
        }

        BotonEnviar.IsEnabled = false;
        Mostrar("Verificando el correo...");

        if (await ValidadorCorreo.ComprobarDominioAsync(para) == EstadoDominio.NoExiste)
        {
            Mostrar("El dominio de ese correo no existe. Revisa que esté bien escrito.");
            BotonEnviar.IsEnabled = true;
            return;
        }

        if (!_correo.Cargar().Configurado)
        {
            var config = App.AppHost.Services.GetRequiredService<CorreoConfigWindow>();
            config.Owner = this;

            if (config.ShowDialog() != true)
            {
                BotonEnviar.IsEnabled = true;
                return;
            }
        }

        string ruta = Path.Combine(Path.GetTempPath(), NombreAdjunto());
        Mostrar("Enviando...");

        try
        {
            await Task.Run(() => _pdf.Generar(_cotizacion, ruta));
            await _correo.EnviarAsync(para, CajaAsunto.Text.Trim(), CajaMensaje.Text, ruta);

            await OfrecerGuardarCorreoAsync(para);
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

    private async Task OfrecerGuardarCorreoAsync(string correo)
    {
        try
        {
            if (await _agenda.ExisteCorreoAsync(correo))
            {
                MessageBox.Show($"La cotización se envió a {correo}.", "Enviado",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var respuesta = MessageBox.Show(
                $"La cotización se envió a {correo}.\n\n¿Deseas guardar este correo para futuras sugerencias?",
                "Enviado", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (respuesta == MessageBoxResult.Yes)
                await _agenda.GuardarCorreoAsync(correo);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"La cotización se envió, pero no se pudo guardar el correo.\n\nDetalle: {ex.Message}",
                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
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