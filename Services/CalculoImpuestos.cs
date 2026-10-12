using POS.Models;

namespace POS.Services;

public record DesgloseVenta(decimal Subtotal, decimal Impuestos, decimal Total, decimal TasaIva);

public static class CalculoImpuestos
{
    public static DesgloseVenta Calcular(decimal sumaLineas, ConfiguracionNegocio configuracion)
    {
        sumaLineas = Math.Round(sumaLineas, 2, MidpointRounding.AwayFromZero);

        if (!configuracion.IvaActivo || configuracion.TasaIva <= 0)
            return new DesgloseVenta(sumaLineas, 0m, sumaLineas, 0m);

        decimal factor = configuracion.TasaIva / 100m;

        if (configuracion.PreciosIncluyenIva)
        {
            decimal subtotal = Math.Round(sumaLineas / (1 + factor), 2, MidpointRounding.AwayFromZero);
            return new DesgloseVenta(subtotal, sumaLineas - subtotal, sumaLineas, configuracion.TasaIva);
        }

        decimal iva = Math.Round(sumaLineas * factor, 2, MidpointRounding.AwayFromZero);
        return new DesgloseVenta(sumaLineas, iva, sumaLineas + iva, configuracion.TasaIva);
    }
}