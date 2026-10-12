using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public partial class CobroViewModel : ObservableObject
{
    public decimal Subtotal { get; }
    public decimal Impuestos { get; }
    public decimal Total { get; }
    public bool MuestraDesglose => Impuestos > 0;

    [ObservableProperty]
    private decimal dineroRecibido;

    [ObservableProperty]
    private MetodoPago metodoSeleccionado = MetodoPago.Efectivo;

    public bool EsEfectivo => MetodoSeleccionado == MetodoPago.Efectivo;
    public bool EsTarjeta => MetodoSeleccionado == MetodoPago.Tarjeta;
    public decimal Cambio => EsEfectivo && DineroRecibido > Total ? DineroRecibido - Total : 0;
    public bool PuedeConfirmar => EsTarjeta || DineroRecibido >= Total;

    public CobroViewModel(DesgloseVenta desglose)
    {
        Subtotal = desglose.Subtotal;
        Impuestos = desglose.Impuestos;
        Total = desglose.Total;
    }

    partial void OnDineroRecibidoChanged(decimal value)
    {
        OnPropertyChanged(nameof(Cambio));
        OnPropertyChanged(nameof(PuedeConfirmar));
    }

    partial void OnMetodoSeleccionadoChanged(MetodoPago value)
    {
        DineroRecibido = value == MetodoPago.Tarjeta ? Total : 0;
        OnPropertyChanged(nameof(EsEfectivo));
        OnPropertyChanged(nameof(EsTarjeta));
        OnPropertyChanged(nameof(Cambio));
        OnPropertyChanged(nameof(PuedeConfirmar));
    }

    [RelayCommand]
    private void SeleccionarEfectivo() => MetodoSeleccionado = MetodoPago.Efectivo;

    [RelayCommand]
    private void SeleccionarTarjeta() => MetodoSeleccionado = MetodoPago.Tarjeta;

    [RelayCommand]
    private void AgregarMonto(string montoTexto)
    {
        if (decimal.TryParse(montoTexto, out decimal monto))
            DineroRecibido += monto;
    }

    [RelayCommand]
    private void PagoExacto() => DineroRecibido = Total;

    [RelayCommand]
    private void LimpiarMonto() => DineroRecibido = 0;
}