using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class AuthService : IAuthService
{
    private readonly PosDbContext _context;

    public AuthService(PosDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ValidarCredencialesAsync(string usuario, string contrasena)
    {
        var encontrado = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.NombreUsuario.ToLower() == usuario.ToLower() && u.Activo);

        if (encontrado is null) return null;

        bool coincide = VerificarContrasena(contrasena, encontrado.ContrasenaHash);
        return coincide ? encontrado : null;
    }

    public async Task<bool> ExisteAlgunUsuarioAsync()
    {
        return await _context.Usuarios.AnyAsync();
    }

    public async Task CrearUsuarioAsync(string usuario, string contrasena)
    {
        var nuevo = new Usuario
        {
            NombreUsuario = usuario.Trim(),
            ContrasenaHash = GenerarHash(contrasena)
        };

        _context.Usuarios.Add(nuevo);
        await _context.SaveChangesAsync();
    }

    private static string GenerarHash(string contrasena)
    {
        byte[] sal = RandomNumberGenerator.GetBytes(16);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(sal)}.{Convert.ToBase64String(hash)}";
    }

    private static bool VerificarContrasena(string contrasena, string hashGuardado)
    {
        var partes = hashGuardado.Split('.');
        if (partes.Length != 2) return false;

        byte[] sal = Convert.FromBase64String(partes[0]);
        byte[] hashEsperado = Convert.FromBase64String(partes[1]);
        byte[] hashIntento = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, 100_000, HashAlgorithmName.SHA256, 32);

        return CryptographicOperations.FixedTimeEquals(hashIntento, hashEsperado);
    }
}