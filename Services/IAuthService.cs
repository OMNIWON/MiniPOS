using POS.Models;

namespace POS.Services;

public interface IAuthService
{
    Task<Usuario?> ValidarCredencialesAsync(string usuario, string contrasena);
    Task<bool> ExisteAlgunUsuarioAsync();
    Task CrearUsuarioAsync(string usuario, string contrasena);
}