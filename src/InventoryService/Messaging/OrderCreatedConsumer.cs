using MassTransit;
using Shared;
using InventoryService.Data;

namespace InventoryService.Messaging
{
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly InventoryDbContext _context;
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(InventoryDbContext context, ILogger<OrderCreatedConsumer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Procesando reducción de stock para la Orden ID: {OrderId}", message.OrderId);

            foreach (var item in message.Items)
            {
                var book = await _context.Books.FindAsync(item.BookId);

                if (book != null)
                {
                    book.StockQuantity -= item.Quantity;
                    
                    _logger.LogInformation("Stock actualizado para el Libro ID {BookId}. Nuevo Stock: {NewStock}", 
                        book.Id, book.StockQuantity);
                }
                else
                {
                    _logger.LogWarning("No se encontró el libro con ID {BookId} para reducir stock.", item.BookId);
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
