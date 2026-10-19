using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using POS.Models;

namespace POS.Services;

public class TicketService : ITicketService
{
    private const double TamanoFuente = 12;

    // Ajuste fino de la posición del texto (en unidades de 1/96 de pulgada; 4 ≈ 1 mm).
    // Si el texto se recorta a la izquierda, sube MargenIzquierdo de 2 en 2.
    private const double MargenIzquierdo = 36;
    private const double MargenDerecho = 4;
    private const double MargenVertical = 4;

    private static readonly FontFamily Fuente = new("Consolas");

    private readonly IConfiguracionService _configuracion;

    public TicketService(IConfiguracionService configuracion)
    {
        _configuracion = configuracion;
    }

    public void Imprimir(Venta venta)
    {
        Enviar($"Ticket venta {venta.Id}",
            columnas => FormatoTicket.Construir(venta, _configuracion.Actual, columnas));
    }

    public void ImprimirPrueba()
    {
        Enviar("Ticket de prueba", columnas => FormatoTicket.ConstruirPrueba(columnas));
    }

    private void Enviar(string titulo, Func<int, List<LineaTicket>> construir)
    {
        var dialogo = new PrintDialog();
        if (dialogo.ShowDialog() != true) return;

        double anchoImprimible = AnchoImprimibleDip(_configuracion.Actual.AnchoTicketMm);
        double anchoPagina = dialogo.PrintableAreaWidth > 0
            ? Math.Min(dialogo.PrintableAreaWidth, anchoImprimible)
            : anchoImprimible;

        int columnas = CalcularColumnas(anchoPagina);

        var documento = CrearDocumento(construir(columnas), anchoPagina);
        documento.PageHeight = dialogo.PrintableAreaHeight > 0 ? dialogo.PrintableAreaHeight : 1100;

        dialogo.PrintDocument(((IDocumentPaginatorSource)documento).DocumentPaginator, titulo);
    }

    // Papel de 58 mm imprime ~48 mm; papel de 80 mm imprime ~72 mm.
    private static double AnchoImprimibleDip(int anchoPapelMm)
    {
        double mm = anchoPapelMm <= 58 ? 48 : 72;
        return mm * 96.0 / 25.4;
    }

    private static int CalcularColumnas(double anchoPagina)
    {
        double anchoCaracter = MedirCaracter();
        double anchoUtil = anchoPagina - MargenIzquierdo - MargenDerecho;
        int columnas = (int)Math.Floor(anchoUtil / (anchoCaracter * 1.02));
        return Math.Max(columnas, 16);
    }

    private static double MedirCaracter()
    {
        var texto = new FormattedText(
            "0",
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            new Typeface(Fuente, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
            TamanoFuente,
            Brushes.Black,
            1.0);

        return texto.WidthIncludingTrailingWhitespace;
    }

    private static FlowDocument CrearDocumento(List<LineaTicket> lineas, double anchoPagina)
    {
        var documento = new FlowDocument
        {
            FontFamily = Fuente,
            FontSize = TamanoFuente,
            Foreground = Brushes.Black,
            PageWidth = anchoPagina,
            PagePadding = new Thickness(MargenIzquierdo, MargenVertical, MargenDerecho, MargenVertical),
            ColumnWidth = anchoPagina - MargenIzquierdo - MargenDerecho,
            ColumnGap = 0,
            TextAlignment = TextAlignment.Left
        };

        var parrafo = new Paragraph { Margin = new Thickness(0) };

        foreach (var linea in lineas)
        {
            parrafo.Inlines.Add(new Run(linea.Texto)
            {
                FontWeight = linea.Negrita ? FontWeights.Bold : FontWeights.Normal
            });
            parrafo.Inlines.Add(new LineBreak());
        }

        for (int i = 0; i < 4; i++)
            parrafo.Inlines.Add(new LineBreak());

        documento.Blocks.Add(parrafo);
        return documento;
    }
}