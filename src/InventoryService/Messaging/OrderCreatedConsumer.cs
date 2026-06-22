using MassTransit;
using Shared;
using System.Linq;
using InventoryService.Services;
using Microsoft.Extensions.Logging;

namespace InventoryService.Messaging
{
    /// <summary>
    /// SINGLE RESPONSIBILITY PRINCIPLE (SRP):
    /// El consumidor tiene la única responsabilidad de actuar como adaptador/puente de mensajería (MassTransit).
    /// Recibe el evento de orden creada y delega el procesamiento de la lógica de negocio al servicio correspondiente.
    /// Ya no realiza transacciones de base de datos ni modifica el estado de la entidad directamente.
    /// 
    /// DEPENDENCY INVERSION PRINCIPLE (DIP):
    /// Depende de la abstracción IBookStockService en lugar de acoplarse con la base de datos (InventoryDbContext).
    /// </summary>
    public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
    {
        private readonly IBookStockService _stockService;
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(IBookStockService stockService, ILogger<OrderCreatedConsumer> logger)
        {
            _stockService = stockService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Recibido evento OrderCreatedEvent para la Orden ID: {OrderId}. Delegando reducción de stock.", message.OrderId);

            // Proyectamos los ítems del evento a tuplas legibles por el servicio
            var itemsToReduce = message.Items.Select(item => (item.BookId, item.Quantity));

            await _stockService.ReduceStockAsync(itemsToReduce, context.CancellationToken);
        }
    }
}

