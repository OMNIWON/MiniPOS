using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public partial class ConfiguracionViewModel : ObservableObject
{
    private readonly IConfiguracionService _servicio;

    [ObservableProperty] private string nombreNegocio = string.Empty;
    [ObservableProperty] private string? direccion;
    [ObservableProperty] private string? telefono;
    [ObservableProperty] private string? mensajeTicket;
    [ObservableProperty] private int anchoTicketMm = 80;
    [ObservableProperty] private bool ivaActivo;
    [ObservableProperty] private decimal tasaIva = 16m;
    [ObservableProperty] private bool preciosIncluyenIva = true;
    [ObservableProperty] private string? mensaje;

    public int[] AnchosDisponibles { get; } = { 58, 80 };

    public event Action? Guardado;

    public ConfiguracionViewModel(IConfiguracionService servicio)
    {
        _servicio = servicio;

        var actual = servicio.Actual;
        NombreNegocio = actual.NombreNegocio;
        Direccion = actual.Direccion;
        Telefono = actual.Telefono;
        MensajeTicket = actual.MensajeTicket;
        AnchoTicketMm = actual.AnchoTicketMm;
        IvaActivo = actual.IvaActivo;
        TasaIva = actual.TasaIva;
        PreciosIncluyenIva = actual.PreciosIncluyenIva;
    }

    [RelayCommand]
    private async Task GuardarAsync()
    {
        if (string.IsNullOrWhiteSpace(NombreNegocio))
        {
            Mensaje = "Escribe el nombre del negocio.";
            return;
        }

        if (TasaIva < 0 || TasaIva > 100)
        {
            Mensaje = "La tasa de IVA debe estar entre 0 y 100.";
            return;
        }

        await _servicio.GuardarAsync(new ConfiguracionNegocio
        {
            NombreNegocio = NombreNegocio.Trim(),
            Direccion = Direccion,
            Telefono = Telefono,
            MensajeTicket = MensajeTicket,
            AnchoTicketMm = AnchoTicketMm,
            IvaActivo = IvaActivo,
            TasaIva = TasaIva,
            PreciosIncluyenIva = PreciosIncluyenIva
        });

        Guardado?.Invoke();
    }
}