using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using POS.Models;

namespace POS.Services;

public class TicketService : ITicketService
{
    private readonly IConfiguracionService _configuracion;

    public TicketService(IConfiguracionService configuracion)
    {
        _configuracion = configuracion;
    }

    public void Imprimir(Venta venta)
    {
        var cfg = _configuracion.Actual;

        var dialogo = new PrintDialog();
        if (dialogo.ShowDialog() != true) return;

        double anchoTicket = cfg.AnchoTicketMm * 96.0 / 25.4;
        double ancho = dialogo.PrintableAreaWidth > 0
            ? Math.Min(dialogo.PrintableAreaWidth, anchoTicket)
            : anchoTicket;

        var documento = Construir(venta, cfg);
        documento.PageWidth = ancho;
        documento.PageHeight = dialogo.PrintableAreaHeight > 0 ? dialogo.PrintableAreaHeight : 1100;
        documento.ColumnWidth = ancho;
        documento.ColumnGap = 0;
        documento.PagePadding = new Thickness(6);

        dialogo.PrintDocument(((IDocumentPaginatorSource)documento).DocumentPaginator, $"Ticket venta {venta.Id}");
    }

    private static FlowDocument Construir(Venta venta, ConfiguracionNegocio cfg)
    {
        var doc = new FlowDocument
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 10,
            Foreground = Brushes.Black
        };

        void Texto(string? texto, double tamano = 10, bool negrita = false, TextAlignment alineacion = TextAlignment.Center)
        {
            if (string.IsNullOrWhiteSpace(texto)) return;
            doc.Blocks.Add(new Paragraph(new Run(texto))
            {
                TextAlignment = alineacion,
                FontSize = tamano,
                FontWeight = negrita ? FontWeights.Bold : FontWeights.Normal,
                Margin = new Thickness(0, 0, 0, 2)
            });
        }

        void Separador() => doc.Blocks.Add(new Paragraph(new Run(" "))
        {
            FontSize = 2,
            Margin = new Thickness(0, 3, 0, 3),
            BorderBrush = Brushes.Black,
            BorderThickness = new Thickness(0, 0, 0, 1)
        });

        Texto(cfg.NombreNegocio, 13, true);
        Texto(cfg.Direccion);
        Texto(string.IsNullOrWhiteSpace(cfg.Telefono) ? null : $"Tel. {cfg.Telefono}");
        Separador();
        Texto($"Ticket No. {venta.Id}", 10, false, TextAlignment.Left);
        Texto($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}", 10, false, TextAlignment.Left);
        Separador();

        var tabla = new Table { CellSpacing = 0 };
        tabla.Columns.Add(new TableColumn { Width = new GridLength(34) });
        tabla.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        tabla.Columns.Add(new TableColumn { Width = new GridLength(76) });
        var grupo = new TableRowGroup();
        tabla.RowGroups.Add(grupo);

        foreach (var d in venta.Detalles)
        {
            var descripcion = new Paragraph(new Run(d.NombreProducto)) { Margin = new Thickness(0) };
            string detalle = $"{d.PrecioUnitario:C2} c/u";
            if (d.DescuentoPorcentaje > 0)
                detalle += $"  -{d.DescuentoPorcentaje:0.##}%";
            descripcion.Inlines.Add(new LineBreak());
            descripcion.Inlines.Add(new Run(detalle) { FontSize = 8 });

            var fila = new TableRow();
            fila.Cells.Add(Celda(d.Cantidad.ToString("0.###"), TextAlignment.Left));
            fila.Cells.Add(new TableCell(descripcion));
            fila.Cells.Add(Celda(d.Subtotal.ToString("C2"), TextAlignment.Right));
            grupo.Rows.Add(fila);
        }
        doc.Blocks.Add(tabla);

        Separador();

        var totales = new Table { CellSpacing = 0 };
        totales.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
        totales.Columns.Add(new TableColumn { Width = new GridLength(90) });
        var filas = new TableRowGroup();
        totales.RowGroups.Add(filas);

        void Linea(string etiqueta, string valor, bool negrita = false)
        {
            var fila = new TableRow();
            fila.Cells.Add(Celda(etiqueta, TextAlignment.Left, negrita));
            fila.Cells.Add(Celda(valor, TextAlignment.Right, negrita));
            filas.Rows.Add(fila);
        }

        if (venta.Impuestos > 0)
        {
            Linea("Subtotal", venta.Subtotal.ToString("C2"));
            Linea($"IVA ({venta.TasaIva:0.##}%)", venta.Impuestos.ToString("C2"));
        }

        Linea("TOTAL", venta.Total.ToString("C2"), true);
        Linea("Pago", venta.MetodoPago == MetodoPago.Tarjeta ? "Tarjeta" : "Efectivo");

        if (venta.MetodoPago == MetodoPago.Efectivo)
        {
            Linea("Recibido", venta.DineroRecibido.ToString("C2"));
            Linea("Cambio", venta.Cambio.ToString("C2"));
        }
        doc.Blocks.Add(totales);

        Separador();
        Texto(cfg.MensajeTicket);

        return doc;
    }

    private static TableCell Celda(string texto, TextAlignment alineacion, bool negrita = false)
    {
        return new TableCell(new Paragraph(new Run(texto))
        {
            Margin = new Thickness(0),
            TextAlignment = alineacion,
            FontWeight = negrita ? FontWeights.Bold : FontWeights.Normal
        });
    }
}