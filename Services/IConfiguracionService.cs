using POS.Models;

namespace POS.Services;

public interface IConfiguracionService
{
    ConfiguracionNegocio Actual { get; }
    void Cargar();
    Task GuardarAsync(ConfiguracionNegocio nueva);
}