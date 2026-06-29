using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data.Repositories
{
    /// <summary>
    /// SINGLE RESPONSIBILITY PRINCIPLE (SRP):
    /// Esta clase tiene la única responsabilidad de interactuar con la base de datos a través de EF Core.
    /// Encapsula las consultas SQL, llamadas a funciones de base de datos y directivas de tracking,
    /// abstrayendo al resto de la aplicación de cómo y dónde se almacenan los datos.
    /// </summary>
    public class BookRepository : IBookRepository
    {
        private readonly InventoryDbContext _context;

        public BookRepository(InventoryDbContext context)
        {
            _context = context;
        }

        // --- Lecturas (CQRS - Optimización AsNoTracking) ---

        public async Task<Book?> GetByIdNoTrackingAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Books
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        }

        public async Task<List<Book>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Books
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<(List<Book> Items, int TotalCount)> GetPagedAsync(
            int? pageNumber, 
            int? pageSize, 
            string? category, 
            string? searchQuery, 
            CancellationToken cancellationToken = default)
        {
            var query = _context.Books.AsNoTracking();

            if (!string.IsNullOrEmpty(category) && !category.Equals("Todas", System.StringComparison.OrdinalIgnoreCase))
            {
                var catLower = category.ToLower();
                if (catLower == "general")
                {
                    query = query.Where(b => string.IsNullOrEmpty(b.Category) || b.Category.ToLower() == "general");
                }
                else
                {
                    query = query.Where(b => b.Category.ToLower() == catLower);
                }
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                query = query.Where(b => b.Title.ToLower().Contains(searchLower) || b.Author.ToLower().Contains(searchLower));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            List<Book> items;
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                items = await query
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToListAsync(cancellationToken);
            }
            else
            {
                items = await query.ToListAsync(cancellationToken);
            }

            return (items, totalCount);
        }

        public async Task<List<Book>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default)
        {
            return await _context.Books
                .AsNoTracking()
                .Where(b => b.StockQuantity < threshold)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<(Book Book, decimal DiscountedPrice)>> GetDiscountedAsync(decimal discountPercentage, CancellationToken cancellationToken = default)
        {
            // OPEN/CLOSED PRINCIPLE (OCP):
            // Si en el futuro decidimos cambiar el origen del cálculo de descuentos (por ejemplo, calcularlo en memoria,
            // leer de otra tabla o usar un motor de reglas), solo modificamos este método en la capa de datos.
            // Los Handlers que ejecutan la consulta no se enteran de cómo se calcula la base imponible o qué función de DB se llama.
            
            var query = _context.Books
                .Select(b => new
                {
                    Book = b,
                    DiscountedPrice = InventoryDbContext.CalculateDiscount(b.Price, discountPercentage)
                });

            var result = await query.ToListAsync(cancellationToken);

            return result.Select(r => (r.Book, r.DiscountedPrice)).ToList();
        }

        // --- Escrituras (CQRS - Operaciones con seguimiento) ---

        public async Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            // FindAsync realiza tracking del objeto por defecto para que podamos actualizarlo o borrarlo después.
            return await _context.Books.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task AddAsync(Book book, CancellationToken cancellationToken = default)
        {
            await _context.Books.AddAsync(book, cancellationToken);
        }

        public void Update(Book book)
        {
            _context.Books.Update(book);
        }

        public void Delete(Book book)
        {
            _context.Books.Remove(book);
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
    }
}
