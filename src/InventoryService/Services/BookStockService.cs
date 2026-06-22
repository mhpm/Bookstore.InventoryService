using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using InventoryService.Data.Repositories;

namespace InventoryService.Services
{
    /// <summary>
    /// SINGLE RESPONSIBILITY PRINCIPLE (SRP):
    /// Esta clase se encarga exclusivamente de orquestar la lógica de negocio y las reglas asociadas al stock de libros,
    /// tales como verificar existencias y registrar actualizaciones en bitácoras. No sabe nada de HTTP ni de colas de mensajería (RabbitMQ).
    /// 
    /// DEPENDENCY INVERSION PRINCIPLE (DIP):
    /// Depende directamente de la abstracción IBookRepository para interactuar con la persistencia, en lugar del DbContext concreto.
    /// </summary>
    public class BookStockService : IBookStockService
    {
        private readonly IBookRepository _repository;
        private readonly ILogger<BookStockService> _logger;

        public BookStockService(IBookRepository repository, ILogger<BookStockService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task ReduceStockAsync(IEnumerable<(int BookId, int Quantity)> items, CancellationToken cancellationToken = default)
        {
            foreach (var item in items)
            {
                var book = await _repository.GetByIdAsync(item.BookId, cancellationToken);

                if (book != null)
                {
                    // OPEN/CLOSED PRINCIPLE (OCP):
                    // Si en el futuro agregamos reglas de negocio complejas (por ejemplo: alertar si el stock es menor a 0,
                    // impedir la reducción si es negativo, o disparar eventos de reabastecimiento automático),
                    // estas reglas se implementan aquí sin alterar los controladores ni los consumidores de eventos.
                    
                    book.StockQuantity -= item.Quantity;
                    
                    _logger.LogInformation("Stock reducido para el Libro ID {BookId} ({Title}). Cantidad restada: {Quantity}. Nuevo Stock: {NewStock}", 
                        book.Id, book.Title, item.Quantity, book.StockQuantity);
                }
                else
                {
                    _logger.LogWarning("No se encontró el libro con ID {BookId} para reducir stock.", item.BookId);
                }
            }

            await _repository.SaveChangesAsync(cancellationToken);
        }
    }
}
