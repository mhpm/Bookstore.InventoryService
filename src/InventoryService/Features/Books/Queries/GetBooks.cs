using MediatR;
using InventoryService.Data.Repositories;

namespace InventoryService.Features.Books.Queries
{

    public record BookDto(
        int Id, 
        string Title, 
        string Author, 
        decimal Price, 
        int StockQuantity
    );

    public record GetBooksQuery() : IRequest<List<BookDto>>;

    /// <summary>
    /// DEPENDENCY INVERSION PRINCIPLE (DIP):
    /// Depende de IBookRepository para obtener la lista de libros en lugar de EF Core directamente.
    /// 
    /// CQRS PATTERN (QUERY):
    /// Representa una Consulta de lectura (Query). No altera el estado del sistema. En el repositorio
    /// se optimiza usando AsNoTracking() para evitar sobrecarga de rendimiento por seguimiento de cambios.
    /// </summary>
    public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, List<BookDto>>
    {
        private readonly IBookRepository _repository;

        public GetBooksQueryHandler(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BookDto>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
        {
            var books = await _repository.GetAllAsync(cancellationToken);

            return books.Select(book => new BookDto(
                book.Id,
                book.Title,
                book.Author,
                book.Price,
                book.StockQuantity
            )).ToList();
        }
    }
}

