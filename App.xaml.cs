using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using POS.Data;
using POS.Services;
using POS.ViewModels;
using System.IO;
using System.Windows;


namespace POS;

public partial class App : Application
{
    public static IHost AppHost { get; private set; } = null!;

    public App()
    {
        AppHost = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                string dbPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "POS", "pos.db");

                Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

                services.AddDbContext<PosDbContext>(options =>
                    options.UseSqlite($"Data Source={dbPath}"));

                services.AddScoped<IProductoService, ProductoService>();
                services.AddScoped<IInventarioService, InventarioService>();
                services.AddScoped<IVentaService, VentaService>();
                services.AddScoped<ICotizacionService, CotizacionService>();
                services.AddScoped<IAuthService, AuthService>();

                services.AddTransient<POS.Views.LoginWindow>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<MainWindow>();
                services.AddTransient<POS.Views.ShellWindow>();
                services.AddTransient<POS.ViewModels.MainViewModel>();
                services.AddTransient<POS.Views.CotizarWindow>();
                services.AddTransient<CotizarViewModel>();
                services.AddTransient<ProductoFormViewModel>();
                services.AddTransient<InventarioViewModel>();
                services.AddTransient<POS.Views.InventarioWindow>();
                services.AddTransient<HistorialViewModel>();
                services.AddTransient<POS.Views.HistorialWindow>();

            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;

        await AppHost.StartAsync();

        using (var scope = AppHost.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PosDbContext>();
            db.Database.Migrate();
            DbInitializer.Seed(db);
        }

        var login = AppHost.Services.GetRequiredService<POS.Views.LoginWindow>();
        bool? resultado = login.ShowDialog();

        if (resultado != true)
        {
            MessageBox.Show($"El inicio de sesión no se completó (resultado: {resultado}). La aplicación se cerrará.",
                "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            Shutdown();
            return;
        }

        try
        {
            var shell = AppHost.Services.GetRequiredService<POS.Views.ShellWindow>();
            shell.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"No se pudo abrir el menú principal.\n\nDetalle: {ex}",
                "Error crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }
    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            "Ocurrió un problema inesperado y la operación no se completó.\n\n" +
            "La información no se ha perdido. Puedes intentar nuevamente.\n\n" +
            $"Detalle técnico: {e.Exception.Message}",
            "Aviso",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);

        e.Handled = true;
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost.StopAsync();
        base.OnExit(e);
    }
}