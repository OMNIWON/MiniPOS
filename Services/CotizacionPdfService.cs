using System.Globalization;
using POS.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;

namespace POS.Services;

public interface ICotizacionPdfService
{
    void Generar(Cotizacion cotizacion, string rutaDestino);
}

public class CotizacionPdfService : ICotizacionPdfService
{
    // Texto al pie de la cotización. Edítalo a tu gusto.
    private const string TextoLegal = "Cotización informativa. Precios sujetos a cambio sin previo aviso.";

    private static readonly CultureInfo Cultura = new("es-MX");

    private readonly IConfiguracionService _configuracion;

    static CotizacionPdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public CotizacionPdfService(IConfiguracionService configuracion)
    {
        _configuracion = configuracion;
    }

    public void Generar(Cotizacion cotizacion, string rutaDestino)
    {
        var cfg = _configuracion.Actual;

        Document.Create(documento =>
        {
            documento.Page(pagina =>
            {
                pagina.Size(PageSizes.Letter);
                pagina.Margin(36);
                pagina.DefaultTextStyle(estilo => estilo.FontSize(10));

                pagina.Header().Element(c => Encabezado(c, cotizacion, cfg));
                pagina.Content().PaddingVertical(14).Element(c => Contenido(c, cotizacion, cfg));

                pagina.Footer().AlignCenter().Text(texto =>
                {
                    texto.Span("Página ").FontSize(9);
                    texto.CurrentPageNumber().FontSize(9);
                    texto.Span(" de ").FontSize(9);
                    texto.TotalPages().FontSize(9);
                });
            });
        }).GeneratePdf(rutaDestino);
    }

    private static readonly Lazy<byte[]?> Logo = new(() =>
    {
        string ruta = Path.Combine(AppContext.BaseDirectory, "Recursos_App", "logo.png");
        return File.Exists(ruta) ? File.ReadAllBytes(ruta) : null;
    });

    private static void Encabezado(IContainer contenedor, Cotizacion cotizacion, ConfiguracionNegocio cfg)
    {
        byte[]? logo = Logo.Value;

        contenedor.Column(columna =>
        {
            columna.Item().Row(fila =>
            {
                fila.RelativeItem().Row(izquierda =>
                {
                    if (logo is not null)
                        izquierda.ConstantItem(78).Height(78).Image(logo).FitArea();

                    izquierda.RelativeItem()
                        .PaddingLeft(logo is null ? 0 : 12)
                        .AlignMiddle()
                        .Column(datos =>
                        {
                            datos.Item().Text(cfg.NombreNegocio.ToUpperInvariant())
                                .FontSize(20).Bold().FontColor(Colors.Grey.Darken4);

                            if (!string.IsNullOrWhiteSpace(cfg.Direccion))
                                datos.Item().Text(cfg.Direccion);

                            if (!string.IsNullOrWhiteSpace(cfg.Telefono))
                                datos.Item().Text($"Tel. {cfg.Telefono}");
                        });
                });

                fila.ConstantItem(170).AlignMiddle().Column(derecha =>
                {
                    derecha.Item().AlignRight().Text("COTIZACIÓN").FontSize(18).Bold().FontColor("#E8772E");
                    derecha.Item().AlignRight().Text($"No. {cotizacion.Id}");
                    derecha.Item().AlignRight().Text(
                        $"Fecha: {cotizacion.Fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)}");
                });
            });

            columna.Item().PaddingTop(8).LineHorizontal(1);
        });
    }

    private static void Contenido(IContainer contenedor, Cotizacion cotizacion, ConfiguracionNegocio cfg)
    {
        contenedor.Column(columna =>
        {
            columna.Spacing(10);

            columna.Item().Text(texto =>
            {
                texto.Span("Cliente: ").SemiBold();
                texto.Span(string.IsNullOrWhiteSpace(cotizacion.Cliente) ? "Público en general" : cotizacion.Cliente);
            });

            columna.Item().Table(tabla =>
            {
                tabla.ColumnsDefinition(columnas =>
                {
                    columnas.RelativeColumn(5);
                    columnas.ConstantColumn(50);
                    columnas.ConstantColumn(72);
                    columnas.ConstantColumn(45);
                    columnas.ConstantColumn(78);
                });

                tabla.Header(cabecera =>
                {
                    cabecera.Cell().Element(CeldaEncabezado).Text("Descripción");
                    cabecera.Cell().Element(CeldaEncabezado).AlignRight().Text("Cant.");
                    cabecera.Cell().Element(CeldaEncabezado).AlignRight().Text("Precio");
                    cabecera.Cell().Element(CeldaEncabezado).AlignRight().Text("Desc.");
                    cabecera.Cell().Element(CeldaEncabezado).AlignRight().Text("Importe");
                });

                foreach (var d in cotizacion.Detalles)
                {
                    string descuento = d.DescuentoPorcentaje > 0
                        ? $"{d.DescuentoPorcentaje.ToString("0.##", Cultura)}%"
                        : "-";

                    tabla.Cell().Element(Celda).Text(d.NombreProducto);
                    tabla.Cell().Element(Celda).AlignRight().Text(d.Cantidad.ToString("0.###", Cultura));
                    tabla.Cell().Element(Celda).AlignRight().Text(FormatoTicket.Dinero(d.PrecioUnitario));
                    tabla.Cell().Element(Celda).AlignRight().Text(descuento);
                    tabla.Cell().Element(Celda).AlignRight().Text(FormatoTicket.Dinero(d.Subtotal));
                }
            });

            columna.Item().AlignRight().Text(texto =>
            {
                texto.Span("TOTAL: ").FontSize(13).SemiBold();
                texto.Span(FormatoTicket.Dinero(cotizacion.Total)).FontSize(13).Bold();
            });

            string? notaIva = NotaIva(cfg);
            if (notaIva is not null)
                columna.Item().AlignRight().Text(notaIva).FontSize(9).FontColor(Colors.Grey.Darken2);

            columna.Item().PaddingTop(10).Text(TextoLegal).FontSize(9).FontColor(Colors.Grey.Darken2);
        });
    }

    private static string? NotaIva(ConfiguracionNegocio cfg)
    {
        if (!cfg.IvaActivo) return null;

        return cfg.PreciosIncluyenIva
            ? "Precios con IVA incluido."
            : $"Importes antes de IVA (más {cfg.TasaIva.ToString("0.##", Cultura)}% de IVA).";
    }

    private static IContainer CeldaEncabezado(IContainer contenedor) =>
        contenedor.Background(Colors.Grey.Lighten3)
                  .PaddingVertical(5).PaddingHorizontal(3)
                  .DefaultTextStyle(estilo => estilo.SemiBold());

    private static IContainer Celda(IContainer contenedor) =>
        contenedor.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2)
                  .PaddingVertical(4).PaddingHorizontal(3);
}