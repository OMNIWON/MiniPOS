namespace POS.Services;

public interface IBackupService
{
    Task<string> CrearRespaldoAsync(string carpetaDestino);
    Task<string?> RespaldoAutomaticoAsync();
}