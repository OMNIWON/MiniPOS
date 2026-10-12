using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using POS.Data;

namespace POS.Services;

public class BackupService : IBackupService
{
    private const int RespaldosAConservar = 14;

    private readonly PosDbContext _context;

    public BackupService(PosDbContext context)
    {
        _context = context;
    }

    public async Task<string> CrearRespaldoAsync(string carpetaDestino)
    {
        string rutaDbOrigen = _context.Database.GetDbConnection().DataSource;
        string marcaTiempo = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string carpetaRespaldo = Path.Combine(carpetaDestino, $"RespaldoPOS_{marcaTiempo}");
        Directory.CreateDirectory(carpetaRespaldo);

        await Task.Run(() =>
        {
            using (var origen = new SqliteConnection($"Data Source={rutaDbOrigen}"))
            using (var destino = new SqliteConnection($"Data Source={Path.Combine(carpetaRespaldo, "pos.db")}"))
            {
                origen.Open();
                destino.Open();
                origen.BackupDatabase(destino);
            }

            string carpetaImagenes = Path.Combine(Path.GetDirectoryName(rutaDbOrigen)!, "Imagenes");
            if (Directory.Exists(carpetaImagenes))
            {
                string destinoImagenes = Path.Combine(carpetaRespaldo, "Imagenes");
                Directory.CreateDirectory(destinoImagenes);
                foreach (var archivo in Directory.GetFiles(carpetaImagenes))
                    File.Copy(archivo, Path.Combine(destinoImagenes, Path.GetFileName(archivo)), overwrite: true);
            }
        });

        return carpetaRespaldo;
    }

    public async Task<string?> RespaldoAutomaticoAsync()
    {
        string raiz = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "POS_Respaldos");
        Directory.CreateDirectory(raiz);

        string prefijoHoy = $"RespaldoPOS_{DateTime.Now:yyyy-MM-dd}";
        foreach (var existente in Directory.GetDirectories(raiz, prefijoHoy + "*"))
            Directory.Delete(existente, recursive: true);

        string carpeta = await CrearRespaldoAsync(raiz);

        var viejos = Directory.GetDirectories(raiz, "RespaldoPOS_*")
            .OrderByDescending(d => d)
            .Skip(RespaldosAConservar);

        foreach (var viejo in viejos)
            Directory.Delete(viejo, recursive: true);

        return carpeta;
    }
}