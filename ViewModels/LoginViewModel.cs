using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS.Models;
using POS.Services;

namespace POS.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string nombreUsuario = string.Empty;

    [ObservableProperty]
    private string mensaje = string.Empty;

    [ObservableProperty]
    private bool esConfiguracionInicial;

    [ObservableProperty]
    private string textoBoton = "Entrar";

    public Usuario? UsuarioAutenticado { get; private set; }

    public event Action? IniciarSesionExitoso;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task InicializarAsync()
    {
        try
        {
            // Si no hay usuarios, el formulario estará en modo
            // de creación explícita del primer usuario.
            EsConfiguracionInicial =
                !await _authService.ExisteAlgunUsuarioAsync();

            TextoBoton = EsConfiguracionInicial
                ? "Crear usuario inicial"
                : "Entrar";

            Mensaje = EsConfiguracionInicial
                ? "Primera configuración: escribe el usuario y la contraseña que deseas crear."
                : string.Empty;
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo comprobar la configuración: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task IniciarSesionAsync(object? parametroContrasena)
    {
        try
        {
            string usuarioIngresado = NombreUsuario.Trim();
            string contrasena =
                parametroContrasena?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(usuarioIngresado) ||
                string.IsNullOrWhiteSpace(contrasena))
            {
                Mensaje = "Escribe usuario y contraseña.";
                return;
            }

            // Solo se crea una cuenta cuando la interfaz está
            // explícitamente en modo de configuración inicial.
            if (EsConfiguracionInicial)
            {
                await _authService.CrearUsuarioAsync(
                    usuarioIngresado,
                    contrasena);

                EsConfiguracionInicial = false;
                TextoBoton = "Entrar";
                Mensaje = string.Empty;
            }

            // Validar las credenciales no consume intentos
            // ni establece bloqueos si la contraseña es incorrecta.
            var usuario = await _authService.ValidarCredencialesAsync(
                usuarioIngresado,
                contrasena);

            if (usuario is null)
            {
                Mensaje = "Usuario o contraseña incorrectos. Inténtalo de nuevo.";
                return;
            }

            UsuarioAutenticado = usuario;
            Mensaje = string.Empty;

            IniciarSesionExitoso?.Invoke();
        }
        catch (Exception ex)
        {
            Mensaje = $"No se pudo completar el inicio de sesión: {ex.Message}";
        }
    }
}
