using MediatR;
using InventoryService.Data;
using InventoryService.Data.Repositories;
using InventoryService.Exceptions;

namespace InventoryService.Features.Books.Commands
{
    public record CreateBookCommand(
        string Title, 
        string Author, 
        decimal Price, 
        int StockQuantity,
        double Rating = 0.0,
        string Category = "General"
    ) : IRequest<int>;

    /// <summary>
    /// DEPENDENCY INVERSION PRINCIPLE (DIP):
    /// Este Handler depende de la abstracción IBookRepository en lugar de acoplarse directamente a EF Core / DbContext.
    /// 
    /// CQRS PATTERN (COMMAND):
    /// Este Handler es exclusivo de la escritura (Command). Recibe una acción que altera el estado del sistema,
    /// crea una nueva entidad que será rastreada y persiste los cambios en la base de datos.
    /// </summary>
    public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, int>
    {
        private readonly IBookRepository _repository;

        public CreateBookCommandHandler(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<int> Handle(CreateBookCommand request, CancellationToken cancellationToken)
        {
            var trimmedTitle = request.Title?.Trim();
            var trimmedAuthor = request.Author?.Trim();

            if (string.IsNullOrWhiteSpace(trimmedTitle))
            {
                throw new ValidationException("El título del libro no puede estar vacío ni contener solo espacios.");
            }

            if (string.IsNullOrWhiteSpace(trimmedAuthor))
            {
                throw new ValidationException("El autor del libro no puede estar vacío ni contener solo espacios.");
            }

            if (trimmedTitle.Contains('<') || trimmedTitle.Contains('>'))
            {
                throw new ValidationException("El título contiene caracteres no permitidos por razones de seguridad.");
            }

            if (trimmedAuthor.Contains('<') || trimmedAuthor.Contains('>'))
            {
                throw new ValidationException("El autor contiene caracteres no permitidos por razones de seguridad.");
            }

            var book = new Book
            {
                Title = trimmedTitle,
                Author = trimmedAuthor,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Rating = request.Rating,
                Category = request.Category
            };

            await _repository.AddAsync(book, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return book.Id;
        }
    }
}

