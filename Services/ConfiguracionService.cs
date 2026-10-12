using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using POS.Data;
using POS.Models;

namespace POS.Services;

public class ConfiguracionService : IConfiguracionService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ConfiguracionNegocio Actual { get; private set; } = new();

    public ConfiguracionService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void Cargar()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PosDbContext>();
        Actual = db.Configuraciones.AsNoTracking().FirstOrDefault() ?? new ConfiguracionNegocio();
    }

    public async Task GuardarAsync(ConfiguracionNegocio nueva)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<PosDbContext>();
            var existente = await db.Configuraciones.FirstOrDefaultAsync();

            if (existente is null)
            {
                nueva.Id = 0;
                db.Configuraciones.Add(nueva);
            }
            else
            {
                nueva.Id = existente.Id;
                db.Entry(existente).CurrentValues.SetValues(nueva);
            }

            await db.SaveChangesAsync();
        }

        Cargar();
    }
}