using System.IO;
using System.Text;

namespace POS.Services;

/// <summary>Registra errores técnicos en un archivo local sin que un fallo del log derribe la aplicación.</summary>
public static class ErrorLogger
{
    private static readonly object Sync = new();

    public static void Log(Exception exception, string origen = "Aplicación")
    {
        try
        {
            string carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "POS", "Logs");
            Directory.CreateDirectory(carpeta);

            string archivo = Path.Combine(carpeta, $"errores-{DateTime.Now:yyyy-MM-dd}.log");
            string entrada = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{origen}]\n{exception}\n{new string('-', 80)}\n";

            lock (Sync)
                File.AppendAllText(archivo, entrada, Encoding.UTF8);
        }
        catch
        {
            // El registro de errores nunca debe provocar otro error.
        }
    }
}