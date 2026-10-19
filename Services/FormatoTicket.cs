using System.Globalization;
using System.Text;
using POS.Models;

namespace POS.Services;

public record LineaTicket(string Texto, bool Negrita = false);

public static class FormatoTicket
{
    private static readonly CultureInfo Cultura = new("es-MX");

    public static string Dinero(decimal valor) => valor.ToString("C2", Cultura);

    public static List<LineaTicket> Construir(Venta venta, ConfiguracionNegocio cfg, int columnas)
    {
        var lineas = new List<LineaTicket>();

        void Agregar(string texto, bool negrita = false) => lineas.Add(new LineaTicket(texto, negrita));

        void Centrado(string? texto, bool negrita = false)
        {
            if (string.IsNullOrWhiteSpace(texto)) return;
            foreach (var l in Envolver(texto.Trim(), columnas))
                Agregar(Centrar(l, columnas), negrita);
        }

        void Izquierda(string texto)
        {
            foreach (var l in Envolver(texto, columnas))
                Agregar(l);
        }

        void Separador() => Agregar(new string('-', columnas));

        Centrado(cfg.NombreNegocio.ToUpperInvariant(), true);
        Centrado(cfg.Direccion);
        if (!string.IsNullOrWhiteSpace(cfg.Telefono))
            Centrado($"Tel. {cfg.Telefono}");
        Separador();
        Izquierda($"Ticket No. {venta.Id}");
        Izquierda($"Fecha: {venta.Fecha.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture)}");
        Separador();

        foreach (var d in venta.Detalles)
        {
            foreach (var l in Envolver(d.NombreProducto, columnas))
                Agregar(l);

            string cantidadYPrecio = $"{d.Cantidad.ToString("0.###", Cultura)} x {Dinero(d.PrecioUnitario)}";
            Agregar(DosColumnas(cantidadYPrecio, Dinero(d.Subtotal), columnas));

            if (d.DescuentoPorcentaje > 0)
                Agregar($"  Desc. {d.DescuentoPorcentaje.ToString("0.##", Cultura)}%");
        }

        Separador();

        if (venta.Impuestos > 0)
        {
            Agregar(DosColumnas("Subtotal", Dinero(venta.Subtotal), columnas));
            Agregar(DosColumnas($"IVA ({venta.TasaIva.ToString("0.##", Cultura)}%)", Dinero(venta.Impuestos), columnas));
        }

        Agregar(DosColumnas("TOTAL", Dinero(venta.Total), columnas), true);
        Agregar(DosColumnas("Pago", venta.MetodoPago == MetodoPago.Tarjeta ? "Tarjeta" : "Efectivo", columnas));

        if (venta.MetodoPago == MetodoPago.Efectivo)
        {
            Agregar(DosColumnas("Recibido", Dinero(venta.DineroRecibido), columnas));
            Agregar(DosColumnas("Cambio", Dinero(venta.Cambio), columnas));
        }

        Separador();
        Centrado(cfg.MensajeTicket);

        return lineas;
    }

    public static List<LineaTicket> ConstruirPrueba(int columnas)
    {
        var lineas = new List<LineaTicket>();

        void Agregar(string texto, bool negrita = false) => lineas.Add(new LineaTicket(texto, negrita));

        void Texto(string texto)
        {
            foreach (var l in Envolver(texto, columnas))
                Agregar(l);
        }

        string regla = string.Concat(Enumerable.Range(1, columnas).Select(i => (i % 10).ToString()));

        Agregar(Centrar("TICKET DE PRUEBA", columnas), true);
        Texto($"Columnas: {columnas}");
        Agregar(regla);
        Agregar(new string('=', columnas));
        Texto("Acentos: áéíóú ñÑ ¿? ¡!");
        Texto("Descripción larga de un artículo para comprobar que el texto se ajusta bien al ancho del papel.");
        Agregar(DosColumnas("2 x $28.00", "$56.00", columnas));
        Agregar(DosColumnas("TOTAL", "$1,234.50", columnas), true);
        Agregar(regla);

        return lineas;
    }

    public static string Centrar(string texto, int columnas)
    {
        if (texto.Length >= columnas) return texto;
        return new string(' ', (columnas - texto.Length) / 2) + texto;
    }

    public static string DosColumnas(string izquierda, string derecha, int columnas)
    {
        if (derecha.Length >= columnas)
            return derecha[..columnas];

        int maximoIzquierda = columnas - derecha.Length - 1;
        if (izquierda.Length > maximoIzquierda)
            izquierda = izquierda[..maximoIzquierda];

        int espacios = columnas - izquierda.Length - derecha.Length;
        return izquierda + new string(' ', espacios) + derecha;
    }

    public static List<string> Envolver(string texto, int ancho)
    {
        var resultado = new List<string>();
        var actual = new StringBuilder();

        foreach (var palabra in texto.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            string p = palabra;

            while (p.Length > ancho)
            {
                if (actual.Length > 0)
                {
                    resultado.Add(actual.ToString());
                    actual.Clear();
                }
                resultado.Add(p[..ancho]);
                p = p[ancho..];
            }

            if (actual.Length == 0)
            {
                actual.Append(p);
            }
            else if (actual.Length + 1 + p.Length <= ancho)
            {
                actual.Append(' ').Append(p);
            }
            else
            {
                resultado.Add(actual.ToString());
                actual.Clear();
                actual.Append(p);
            }
        }

        if (actual.Length > 0)
            resultado.Add(actual.ToString());

        if (resultado.Count == 0)
            resultado.Add(string.Empty);

        return resultado;
    }
}