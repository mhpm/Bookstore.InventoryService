using MediatR;
using InventoryService.Data;
using InventoryService.Data.Repositories;
using Mapster;

namespace InventoryService.Features.Books.Commands
{
    public record UpdateBookCommand(
        int Id,
        string? Title = null,
        string? Author = null,
        decimal? Price = null,
        int? StockQuantity = null,
        double? Rating = null,
        string? Category = null
    ) : IRequest<bool>;

    /// <summary>
    /// DEPENDENCY INVERSION PRINCIPLE (DIP):
    /// El Handler consume IBookRepository (abstracción) para realizar la modificación de la entidad,
    /// ocultando los detalles internos de base de datos.
    /// 
    /// CQRS PATTERN (COMMAND):
    /// Representa un Comando de modificación. Obtiene una entidad bajo seguimiento (Tracking), aplica
    /// los cambios y los guarda en la base de datos de manera transaccional.
    /// </summary>
    public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, bool>
    {
        private readonly IBookRepository _repository;

        public UpdateBookCommandHandler(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
        {
            var book = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (book == null)
            {
                return false; 
            }

            var config = new TypeAdapterConfig();
            
            config.NewConfig<UpdateBookCommand, Book>()
                  .IgnoreNullValues(true);

            request.Adapt(book, config);

            await _repository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

