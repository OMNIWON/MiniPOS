using CommunityToolkit.Mvvm.ComponentModel;
using POS.Models;

namespace POS.ViewModels;

public partial class CarritoItemViewModel : ObservableObject
{
    public Producto Producto { get; }

    [ObservableProperty]
    private decimal cantidad;

    [ObservableProperty]
    private decimal precioUnitario;

    private decimal descuentoPorcentaje;
    public decimal DescuentoPorcentaje
    {
        get => descuentoPorcentaje;
        set
        {
            decimal limitado = value;
            if (limitado < 0) limitado = 0;
            if (limitado > 100) limitado = 100;

            if (SetProperty(ref descuentoPorcentaje, limitado))
                OnPropertyChanged(nameof(Subtotal));
        }
    }

    public CarritoItemViewModel(Producto producto, decimal cantidadInicial)
    {
        Producto = producto;
        cantidad = cantidadInicial;
        precioUnitario = producto.PrecioVenta;
        descuentoPorcentaje = 0m;
    }

    public decimal Subtotal
    {
        get
        {
            decimal bruto = Cantidad * PrecioUnitario;
            decimal factor = 1 - (DescuentoPorcentaje / 100m);
            decimal neto = bruto * factor;
            return neto < 0 ? 0 : Math.Round(neto, 2, MidpointRounding.AwayFromZero);
        }
    }

    partial void OnCantidadChanged(decimal value) => OnPropertyChanged(nameof(Subtotal));
    partial void OnPrecioUnitarioChanged(decimal value) => OnPropertyChanged(nameof(Subtotal));
}