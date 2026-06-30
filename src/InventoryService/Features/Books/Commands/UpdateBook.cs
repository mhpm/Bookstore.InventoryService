using MediatR;
using InventoryService.Data;
using InventoryService.Data.Repositories;
using Mapster;
using InventoryService.Exceptions;

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

            var commandToProcess = request;

            if (request.Title != null)
            {
                var trimmedTitle = request.Title.Trim();
                if (string.IsNullOrWhiteSpace(trimmedTitle))
                {
                    throw new ValidationException("El título del libro no puede estar vacío ni contener solo espacios.");
                }
                if (trimmedTitle.Contains('<') || trimmedTitle.Contains('>'))
                {
                    throw new ValidationException("El título contiene caracteres no permitidos por razones de seguridad.");
                }
                commandToProcess = commandToProcess with { Title = trimmedTitle };
            }

            if (request.Author != null)
            {
                var trimmedAuthor = request.Author.Trim();
                if (string.IsNullOrWhiteSpace(trimmedAuthor))
                {
                    throw new ValidationException("El autor del libro no puede estar vacío ni contener solo espacios.");
                }
                if (trimmedAuthor.Contains('<') || trimmedAuthor.Contains('>'))
                {
                    throw new ValidationException("El autor contiene caracteres no permitidos por razones de seguridad.");
                }
                commandToProcess = commandToProcess with { Author = trimmedAuthor };
            }

            var config = new TypeAdapterConfig();
            
            config.NewConfig<UpdateBookCommand, Book>()
                  .IgnoreNullValues(true);

            commandToProcess.Adapt(book, config);

            await _repository.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

