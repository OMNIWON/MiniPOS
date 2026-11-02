using Microsoft.EntityFrameworkCore;
using POS.Data;
using POS.Models;

namespace POS.Services;

public interface IAgendaService
{
    Task<List<string>> ObtenerClientesAsync();
    Task<bool> ExisteClienteAsync(string nombre);
    Task GuardarClienteAsync(string nombre);
    Task<List<string>> ObtenerCorreosAsync();
    Task<bool> ExisteCorreoAsync(string email);
    Task GuardarCorreoAsync(string email);
}

public class AgendaService : IAgendaService
{
    private readonly PosDbContext _context;

    public AgendaService(PosDbContext context)
    {
        _context = context;
    }

    public async Task<List<string>> ObtenerClientesAsync() =>
        await _context.ClientesGuardados.AsNoTracking()
            .OrderBy(c => c.Nombre).Select(c => c.Nombre).ToListAsync();

    public async Task<bool> ExisteClienteAsync(string nombre)
    {
        nombre = nombre.Trim();
        return await _context.ClientesGuardados.AnyAsync(c => c.Nombre == nombre);
    }

    public async Task GuardarClienteAsync(string nombre)
    {
        nombre = nombre.Trim();
        if (nombre.Length > 120) nombre = nombre[..120];
        if (nombre.Length == 0 || await ExisteClienteAsync(nombre)) return;

        var cliente = new ClienteGuardado { Nombre = nombre };
        _context.ClientesGuardados.Add(cliente);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _context.Entry(cliente).State = EntityState.Detached;
        }
    }

    public async Task<List<string>> ObtenerCorreosAsync() =>
        await _context.CorreosGuardados.AsNoTracking()
            .OrderBy(c => c.Email).Select(c => c.Email).ToListAsync();

    public async Task<bool> ExisteCorreoAsync(string email)
    {
        email = email.Trim();
        return await _context.CorreosGuardados.AnyAsync(c => c.Email == email);
    }

    public async Task GuardarCorreoAsync(string email)
    {
        email = email.Trim();
        if (email.Length == 0 || await ExisteCorreoAsync(email)) return;

        var correo = new CorreoGuardado { Email = email };
        _context.CorreosGuardados.Add(correo);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            _context.Entry(correo).State = EntityState.Detached;
        }
    }
}