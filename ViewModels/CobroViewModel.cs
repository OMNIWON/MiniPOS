using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace POS.ViewModels;

public partial class CobroViewModel : ObservableObject
{
    public decimal Total { get; }

    [ObservableProperty]
    private decimal dineroRecibido;

    public decimal Cambio => DineroRecibido > Total ? DineroRecibido - Total : 0;

    public bool PuedeConfirmar => DineroRecibido >= Total;

    public CobroViewModel(decimal total)
    {
        Total = total;
    }

    partial void OnDineroRecibidoChanged(decimal value)
    {
        OnPropertyChanged(nameof(Cambio));
        OnPropertyChanged(nameof(PuedeConfirmar));
    }

    [RelayCommand]
    private void AgregarMonto(string montoTexto)
    {
        if (decimal.TryParse(montoTexto, out decimal monto))
            DineroRecibido += monto;
    }

    [RelayCommand]
    private void PagoExacto()
    {
        DineroRecibido = Total;
    }

    [RelayCommand]
    private void LimpiarMonto()
    {
        DineroRecibido = 0;
    }
}