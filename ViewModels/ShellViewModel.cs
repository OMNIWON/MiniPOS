using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using POS.Services;

namespace POS.ViewModels;

public partial class ShellViewModel : ObservableObject
{
    private readonly IVentaService _ventaService;
    private readonly IProductoService _productoService;
    private readonly IBackupService _backupService;

    [ObservableProperty]
    private decimal ventasHoy;

    [ObservableProperty]
    private int productosStockBajo;

    public ShellViewModel(IVentaService ventaService, IProductoService productoService, IBackupService backupService)
    {
        _ventaService = ventaService;
        _productoService = productoService;
        _backupService = backupService;
        _ = RefrescarAsync();
    }

    [RelayCommand]
    public async Task RefrescarAsync()
    {
        try
        {
            VentasHoy = await _ventaService.ObtenerTotalVentasHoyAsync();
            ProductosStockBajo = await _productoService.ContarStockBajoAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo actualizar el resumen del día.\n\nDetalle: {ex.Message}",
                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    [RelayCommand]
    private async Task RespaldarAsync()
    {
        var dialogo = new OpenFolderDialog { Title = "Elige dónde guardar el respaldo (USB o carpeta)" };
        if (dialogo.ShowDialog() != true) return;

        try
        {
            string carpeta = await _backupService.CrearRespaldoAsync(dialogo.FolderName);
            MessageBox.Show($"Respaldo creado correctamente en:\n\n{carpeta}",
                "Respaldo listo", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo crear el respaldo.\n\nDetalle: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}