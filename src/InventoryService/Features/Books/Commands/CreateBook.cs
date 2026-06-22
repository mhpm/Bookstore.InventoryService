using MediatR;
using InventoryService.Data;
using InventoryService.Data.Repositories;

namespace InventoryService.Features.Books.Commands
{
    public record CreateBookCommand(
        string Title, 
        string Author, 
        decimal Price, 
        int StockQuantity
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
            var book = new Book
            {
                Title = request.Title,
                Author = request.Author,
                Price = request.Price,
                StockQuantity = request.StockQuantity
            };

            await _repository.AddAsync(book, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            return book.Id;
        }
    }
}

