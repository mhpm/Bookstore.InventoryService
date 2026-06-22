using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryService.Services
{
    /// <summary>
    /// SINGLE RESPONSIBILITY PRINCIPLE (SRP) / INTERFACE SEGREGATION PRINCIPLE (ISP):
    /// Interfaz que encapsula únicamente las reglas de negocio correspondientes al stock del inventario.
    /// Aísla el procesamiento de mensajería (MassTransit) de las políticas de cambio de existencias.
    /// </summary>
    public interface IBookStockService
    {
        /// <summary>
        /// Reduce el stock de múltiples libros de manera transaccional.
        /// </summary>
        /// <param name="items">Colección de tuplas conteniendo el ID del libro y la cantidad a reducir.</param>
        Task ReduceStockAsync(IEnumerable<(int BookId, int Quantity)> items, CancellationToken cancellationToken = default);
    }
}
