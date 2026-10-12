using POS.Models;

namespace POS.Services;

/// <summary>Transfiere una cotización seleccionada a una nueva ventana de venta sin alterar el registro original.</summary>
public sealed class CotizacionTransferService
{
    private Cotizacion? _pendiente;
    public void Preparar(Cotizacion cotizacion) => _pendiente = cotizacion;
    public Cotizacion? Consumir()
    {
        var resultado = _pendiente;
        _pendiente = null;
        return resultado;
    }
}