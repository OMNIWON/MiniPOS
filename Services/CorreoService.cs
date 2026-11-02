using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace POS.Services;

public class CorreoConfig
{
    public string Servidor { get; set; } = "smtp.gmail.com";
    public int Puerto { get; set; } = 587;
    public string Usuario { get; set; } = string.Empty;
    public string NombreRemitente { get; set; } = string.Empty;
    public string PasswordProtegida { get; set; } = string.Empty;

    [JsonIgnore]
    public bool Configurado =>
        !string.IsNullOrWhiteSpace(Usuario) && !string.IsNullOrWhiteSpace(PasswordProtegida);
}

public interface ICorreoService
{
    CorreoConfig Cargar();
    void Guardar(CorreoConfig config, string? passwordNueva);
    Task EnviarAsync(string destinatario, string asunto, string cuerpo, string? rutaAdjunto);
}

public class CorreoService : ICorreoService
{
    private static readonly string Ruta = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "POS", "correo.json");

    public CorreoConfig Cargar()
    {
        try
        {
            if (!File.Exists(Ruta)) return new CorreoConfig();
            return JsonSerializer.Deserialize<CorreoConfig>(File.ReadAllText(Ruta)) ?? new CorreoConfig();
        }
        catch
        {
            return new CorreoConfig();
        }
    }

    public void Guardar(CorreoConfig config, string? passwordNueva)
    {
        if (!string.IsNullOrEmpty(passwordNueva))
        {
            byte[] protegida = ProtectedData.Protect(
                Encoding.UTF8.GetBytes(passwordNueva), null, DataProtectionScope.CurrentUser);
            config.PasswordProtegida = Convert.ToBase64String(protegida);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Ruta)!);
        File.WriteAllText(Ruta, JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true }));
    }

    public async Task EnviarAsync(string destinatario, string asunto, string cuerpo, string? rutaAdjunto)
    {
        var config = Cargar();
        if (!config.Configurado)
            throw new InvalidOperationException("El correo de envío todavía no está configurado.");

        string password;
        try
        {
            password = Encoding.UTF8.GetString(ProtectedData.Unprotect(
                Convert.FromBase64String(config.PasswordProtegida), null, DataProtectionScope.CurrentUser));
        }
        catch (CryptographicException)
        {
            throw new InvalidOperationException(
                "No se pudo leer la contraseña guardada (puede haber cambiado el usuario de Windows). Vuelve a configurar el correo.");
        }

        using var mensaje = new MailMessage
        {
            From = new MailAddress(config.Usuario,
                string.IsNullOrWhiteSpace(config.NombreRemitente) ? config.Usuario : config.NombreRemitente),
            Subject = asunto,
            SubjectEncoding = Encoding.UTF8,
            Body = cuerpo,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = false
        };

        mensaje.To.Add(new MailAddress(destinatario));

        if (!string.IsNullOrWhiteSpace(rutaAdjunto))
            mensaje.Attachments.Add(new Attachment(rutaAdjunto));

        using var smtp = new SmtpClient(config.Servidor, config.Puerto)
        {
            EnableSsl = true,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(config.Usuario, password)
        };

        var envio = smtp.SendMailAsync(mensaje);
        var terminado = await Task.WhenAny(envio, Task.Delay(TimeSpan.FromSeconds(45)));

        if (terminado != envio)
            throw new TimeoutException("El servidor de correo tardó demasiado en responder. Revisa tu conexión a internet.");

        await envio;
    }

    public static string Explicar(Exception ex)
    {
        if (ex is InvalidOperationException || ex is TimeoutException)
            return ex.Message;

        string detalle = ex.InnerException?.Message ?? ex.Message;

        if (detalle.Contains("5.7.9") || detalle.Contains("5.7.8") ||
            detalle.Contains("Authentication", StringComparison.OrdinalIgnoreCase))
        {
            return "El correo rechazó el acceso. Con Gmail debes usar una contraseña de aplicación " +
                   "(16 letras), no tu contraseña normal, y tener activada la verificación en 2 pasos.\n\n" +
                   "Detalle: " + detalle;
        }

        return "No se pudo enviar el correo. Revisa tu conexión a internet y los datos del correo.\n\n" +
               "Detalle: " + detalle;
    }
}