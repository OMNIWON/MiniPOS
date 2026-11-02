using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace POS.Services;

public enum EstadoDominio { Existe, NoExiste, NoVerificable }

public record ResultadoCorreo(bool EsValido, string? Mensaje = null, string? Sugerencia = null);

public static class ValidadorCorreo
{
    // Estructura: parte local sin puntos al inicio/fin/dobles, dominio con etiquetas válidas y TLD de letras.
    private static readonly Regex Patron = new(
        @"^(?=.{3,254}$)(?=[^@]{1,64}@)" +
        @"[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[A-Za-z0-9!#$%&'*+/=?^_`{|}~-]+)*" +
        @"@(?:[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z]{2,24}$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled,
        TimeSpan.FromMilliseconds(250));

    private static readonly string[] DominiosComunes =
    {
        "gmail.com", "hotmail.com", "outlook.com", "yahoo.com", "yahoo.com.mx",
        "hotmail.es", "outlook.es", "icloud.com", "prodigy.net.mx", "protonmail.com"
    };

    // Dominios reales parecidos a los comunes: no se marcan como error. Agrega aquí los que necesites.
    private static readonly HashSet<string> DominiosPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "mail.com", "email.com", "live.com", "live.com.mx", "msn.com", "me.com", "aol.com"
    };

    public static ResultadoCorreo Validar(string? correo)
    {
        string texto = (correo ?? string.Empty).Trim();

        if (texto.Length == 0)
            return new ResultadoCorreo(false, "Escribe el correo del cliente.");

        bool formatoValido;
        try
        {
            formatoValido = Patron.IsMatch(texto);
        }
        catch (RegexMatchTimeoutException)
        {
            formatoValido = false;
        }

        if (!formatoValido)
            return new ResultadoCorreo(false, "El correo no tiene un formato válido. Ejemplo: nombre@dominio.com");

        int arroba = texto.LastIndexOf('@');
        string usuario = texto[..arroba];
        string dominio = texto[(arroba + 1)..].ToLowerInvariant();

        string? sugerido = SugerirDominio(dominio);
        if (sugerido is not null)
            return new ResultadoCorreo(false, $"El dominio \"{dominio}\" parece tener un error.", $"{usuario}@{sugerido}");

        return new ResultadoCorreo(true);
    }

    // Comprueba en internet que el dominio exista (registros MX; si no hay, registro A).
    // Nunca bloquea por errores de red: en ese caso devuelve NoVerificable.
    public static async Task<EstadoDominio> ComprobarDominioAsync(string correo)
    {
        int arroba = correo.LastIndexOf('@');
        if (arroba < 0) return EstadoDominio.NoExiste;

        string dominio = correo[(arroba + 1)..].Trim();

        try
        {
            int estado = await Task.Run(() => DominioDns.ConsultarMx(dominio));

            if (estado == 0) return EstadoDominio.Existe;
            if (estado == 9003) return EstadoDominio.NoExiste;      // el dominio no existe
            if (estado != 9501) return EstadoDominio.NoVerificable; // timeout, servidor DNS caído, etc.

            // Sin registros MX: el correo aún puede entregarse si el dominio tiene dirección (registro A).
            try
            {
                var direcciones = await Dns.GetHostAddressesAsync(dominio);
                return direcciones.Length > 0 ? EstadoDominio.Existe : EstadoDominio.NoExiste;
            }
            catch (SocketException ex) when (ex.SocketErrorCode is SocketError.HostNotFound or SocketError.NoData)
            {
                return EstadoDominio.NoExiste;
            }
        }
        catch
        {
            return EstadoDominio.NoVerificable;
        }
    }

    private static string? SugerirDominio(string dominio)
    {
        if (DominiosPermitidos.Contains(dominio) || DominiosComunes.Contains(dominio))
            return null;

        string? mejor = null;
        int menor = int.MaxValue;

        foreach (var candidato in DominiosComunes)
        {
            int distancia = Distancia(dominio, candidato);
            if (distancia < menor)
            {
                menor = distancia;
                mejor = candidato;
            }
        }

        return menor <= 2 ? mejor : null;
    }

    // Distancia de Levenshtein: número de cambios para convertir una cadena en otra.
    private static int Distancia(string a, string b)
    {
        var fila = new int[b.Length + 1];
        for (int j = 0; j <= b.Length; j++) fila[j] = j;

        for (int i = 1; i <= a.Length; i++)
        {
            int anterior = fila[0];
            fila[0] = i;

            for (int j = 1; j <= b.Length; j++)
            {
                int temporal = fila[j];
                int costo = a[i - 1] == b[j - 1] ? 0 : 1;
                fila[j] = Math.Min(Math.Min(fila[j] + 1, fila[j - 1] + 1), anterior + costo);
                anterior = temporal;
            }
        }

        return fila[b.Length];
    }
}

internal static class DominioDns
{
    private const ushort TipoMx = 15;
    private const uint ConsultaEstandar = 0;

    [DllImport("dnsapi.dll", EntryPoint = "DnsQuery_W", CharSet = CharSet.Unicode, ExactSpelling = true)]
    private static extern int DnsQuery(string nombre, ushort tipo, uint opciones, IntPtr extra,
        out IntPtr resultados, IntPtr reservado);

    [DllImport("dnsapi.dll", EntryPoint = "DnsRecordListFree", ExactSpelling = true)]
    private static extern void DnsRecordListFree(IntPtr lista, int tipoLiberacion);

    public static int ConsultarMx(string dominio)
    {
        int estado = DnsQuery(dominio, TipoMx, ConsultaEstandar, IntPtr.Zero, out IntPtr resultados, IntPtr.Zero);

        if (resultados != IntPtr.Zero)
        {
            try
            {
                DnsRecordListFree(resultados, 1);
            }
            catch
            {
                // Liberar la memoria no es crítico.
            }
        }

        return estado;
    }
}