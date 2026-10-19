using POS.Models;

namespace POS.Services;

public interface ITicketService
{
    void Imprimir(Venta venta);
    void ImprimirPrueba();
}