using System.Net.Mail;
using System.Windows;
using POS.Services;

namespace POS.Views;

public partial class CorreoConfigWindow : Window
{
    private readonly ICorreoService _correo;
    private readonly CorreoConfig _config;

    public CorreoConfigWindow(ICorreoService correo)
    {
        InitializeComponent();
        _correo = correo;
        _config = correo.Cargar();

        CajaCorreo.Text = _config.Usuario;
        CajaNombre.Text = _config.NombreRemitente;
        CajaServidor.Text = _config.Servidor;
        CajaPuerto.Text = _config.Puerto.ToString();

        TextoEstado.Text = _config.Configurado
            ? "Ya hay una contraseña guardada. Déjala vacía para conservarla."
            : string.Empty;
    }

    private async void GuardarYProbar_Click(object sender, RoutedEventArgs e)
    {
        string correo = CajaCorreo.Text.Trim();
        string servidor = CajaServidor.Text.Trim();
        string password = CajaPassword.Password.Replace(" ", "");

        if (!MailAddress.TryCreate(correo, out _) || !correo.Contains('@'))
        {
            Mostrar("Escribe un correo válido.");
            return;
        }

        if (servidor.Length == 0)
        {
            Mostrar("Escribe el servidor (para Gmail: smtp.gmail.com).");
            return;
        }

        if (!int.TryParse(CajaPuerto.Text.Trim(), out int puerto) || puerto <= 0 || puerto > 65535)
        {
            Mostrar("El puerto debe ser un número (para Gmail: 587).");
            return;
        }

        if (password.Length == 0 && !_config.Configurado)
        {
            Mostrar("Escribe la contraseña de aplicación.");
            return;
        }

        _config.Usuario = correo;
        _config.NombreRemitente = CajaNombre.Text.Trim();
        _config.Servidor = servidor;
        _config.Puerto = puerto;

        BotonGuardar.IsEnabled = false;
        Mostrar("Guardando y enviando un correo de prueba a tu propia cuenta...");

        try
        {
            _correo.Guardar(_config, password.Length > 0 ? password : null);

            await _correo.EnviarAsync(correo, "Prueba de correo - Punto de Venta",
                "Si recibes este mensaje, el correo quedó configurado correctamente.", null);

            MessageBox.Show("Listo. Te enviamos un correo de prueba; revisa tu bandeja de entrada.",
                "Correo configurado", MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
        }
        catch (Exception ex)
        {
            Mostrar(CorreoService.Explicar(ex));
            BotonGuardar.IsEnabled = true;
        }
    }

    private void Mostrar(string mensaje)
    {
        TextoMensaje.Text = mensaje;
        TextoMensaje.Visibility = Visibility.Visible;
    }
}