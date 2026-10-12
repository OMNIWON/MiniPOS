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

    public Usuario? UsuarioAutenticado { get; private set; }
    public event Action? IniciarSesionExitoso;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [RelayCommand]
    private async Task IniciarSesionAsync(object parametroContrasena)
    {
        try
        {
            string contrasena = parametroContrasena?.ToString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(NombreUsuario) || string.IsNullOrWhiteSpace(contrasena))
            {
                Mensaje = "Escribe usuario y contraseña.";
                return;
            }

            bool hayUsuarios = await _authService.ExisteAlgunUsuarioAsync();

            if (!hayUsuarios)
            {
                await _authService.CrearUsuarioAsync(NombreUsuario, contrasena);
            }

            var usuario = await _authService.ValidarCredencialesAsync(NombreUsuario, contrasena);

            if (usuario is null)
            {
                Mensaje = "Usuario o contraseña incorrectos.";
                return;
            }

            UsuarioAutenticado = usuario;
            IniciarSesionExitoso?.Invoke();
        }
        catch (Exception ex)
        {
            Mensaje = $"Error interno: {ex.Message}";
        }
    }
}