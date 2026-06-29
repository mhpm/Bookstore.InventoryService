using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryService.Data.Repositories
{
    /// <summary>
    /// INTERFACE SEGREGATION PRINCIPLE (ISP):
    /// Esta interfaz contiene un conjunto cohesivo de métodos enfocados exclusivamente en la gestión de persistencia de libros.
    /// Evitamos interfaces redundantes o sobrecargadas con múltiples responsabilidades del negocio.
    /// 
    /// DEPENDENCY INVERSION PRINCIPLE (DIP):
    /// Los componentes de alto nivel (Handlers de MediatR, Servicios de Dominio) dependerán de esta abstracción,
    /// eliminando el acoplamiento directo con el DbContext de EF Core.
    /// </summary>
    public interface IBookRepository
    {
        // --- CQRS Read Operations (Consultas / Queries) ---
        // Optimizadas para lecturas rápidas sin seguimiento de cambios (NoTracking).
        
        Task<Book?> GetByIdNoTrackingAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Book>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<(List<Book> Items, int TotalCount)> GetPagedAsync(
            int? pageNumber, 
            int? pageSize, 
            string? category, 
            string? searchQuery, 
            CancellationToken cancellationToken = default);
        Task<List<Book>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default);
        Task<List<(Book Book, decimal DiscountedPrice)>> GetDiscountedAsync(decimal discountPercentage, CancellationToken cancellationToken = default);

        // --- CQRS Write Operations (Comandos / Commands) ---
        // Operaciones que modifican estado y requieren seguimiento de cambios (Tracking) para transacciones.

        Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task AddAsync(Book book, CancellationToken cancellationToken = default);
        void Update(Book book);
        void Delete(Book book);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
