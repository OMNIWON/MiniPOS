using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using POS.Data;
using POS.Services;
using POS.ViewModels;

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
                services.AddSingleton<CotizacionTransferService>();

                services.AddScoped<IAuthService, AuthService>();
                services.AddScoped<IBackupService, BackupService>();
                services.AddSingleton<IConfiguracionService, ConfiguracionService>();
                services.AddSingleton<ITicketService, TicketService>();

                services.AddTransient<MainWindow>();
                services.AddTransient<MainViewModel>();
                services.AddTransient<POS.Views.ShellWindow>();
                services.AddTransient<ShellViewModel>();
                services.AddTransient<POS.Views.CotizarWindow>();
                services.AddTransient<CotizarViewModel>();
                services.AddTransient<POS.Views.InventarioWindow>();
                services.AddTransient<InventarioViewModel>();
                services.AddTransient<ProductoFormViewModel>();
                services.AddTransient<POS.Views.HistorialWindow>();
                services.AddTransient<HistorialViewModel>();
                services.AddTransient<POS.Views.LoginWindow>();
                services.AddTransient<LoginViewModel>();
                services.AddTransient<POS.Views.ConfiguracionWindow>();
                services.AddTransient<ConfiguracionViewModel>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            ErrorLogger.Log(args.ExceptionObject as Exception ?? new Exception("Excepción no administrada."), "AppDomain");
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            ErrorLogger.Log(args.Exception, "Tarea asíncrona no observada");
            args.SetObserved();
        };

        var splash = new POS.Views.SplashWindow();
        splash.Show();

        try
        {
            await AppHost.StartAsync();

            await Task.Run(() =>
            {
                using var scope = AppHost.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<PosDbContext>();
                db.Database.Migrate();
                DbInitializer.Seed(db);
                string carpetaImagenes = Path.Combine(
                 Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "POS", "Imagenes");

                foreach (var producto in db.Productos.Where(prod => prod.RutaImagen != null))
                {
                    string nueva = Path.Combine(carpetaImagenes, Path.GetFileName(producto.RutaImagen!));
                    if (producto.RutaImagen != nueva)
                        producto.RutaImagen = nueva;
                }
                db.SaveChanges();
                AppHost.Services.GetRequiredService<IConfiguracionService>().Cargar();
            });

            await Task.Delay(1200);
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "Inicio de aplicación / base de datos");
            splash.Close();
            MessageBox.Show($"No se pudo iniciar la base de datos.\n\nDetalle: {ex.Message}",
                "Error al iniciar", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        splash.Close();

        var login = AppHost.Services.GetRequiredService<POS.Views.LoginWindow>();
        bool? resultado = login.ShowDialog();

        if (resultado != true)
        {
            Shutdown();
            return;
        }

        try
        {
            var shell = AppHost.Services.GetRequiredService<POS.Views.ShellWindow>();
            shell.Closed += (s, args) => Shutdown();
            shell.Show();
        }
        catch (Exception ex)
        {
            ErrorLogger.Log(ex, "Apertura del menú principal");
            MessageBox.Show($"No se pudo abrir el menú principal.\n\nDetalle: {ex.Message}",
                "Error crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        ErrorLogger.Log(e.Exception, "Dispatcher WPF");

        MessageBox.Show(
            "Ocurrió un problema inesperado. La operación podría no haberse completado.\n\n" +
            "El error quedó registrado. Si vuelve a ocurrir, avisa al responsable del sistema.\n\n" +
            $"Detalle: {e.Exception.Message}",
            "Aviso",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);

        e.Handled = true;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        try
        {
            Task.Run(async () =>
            {
                using var scope = AppHost.Services.CreateScope();
                var backup = scope.ServiceProvider.GetRequiredService<IBackupService>();
                await backup.RespaldoAutomaticoAsync();
            }).GetAwaiter().GetResult();
        }
        catch
        {
            // Un fallo en el respaldo nunca debe impedir cerrar el programa.
        }

        Task.Run(() => AppHost.StopAsync()).GetAwaiter().GetResult();
        base.OnExit(e);
    }
}