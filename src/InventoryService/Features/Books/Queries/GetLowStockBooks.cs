using MediatR;
using InventoryService.Data.Repositories;

namespace InventoryService.Features.Books.Queries
{
    public record GetLowStockBooksQuery(int Threshold) : IRequest<List<BookDto>>;

    /// <summary>
    /// DEPENDENCY INVERSION PRINCIPLE (DIP):
    /// Se desacopla de la base de datos inyectando IBookRepository.
    /// 
    /// CQRS PATTERN (QUERY):
    /// Consulta que obtiene libros con existencias inferiores a un umbral dado. Es de solo lectura
    /// y utiliza GetLowStockAsync del repositorio con AsNoTracking para mejor desempeño.
    /// </summary>
    public class GetLowStockBooksQueryHandler : IRequestHandler<GetLowStockBooksQuery, List<BookDto>>
    {
        private readonly IBookRepository _repository;

        public GetLowStockBooksQueryHandler(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BookDto>> Handle(GetLowStockBooksQuery request, CancellationToken cancellationToken)
        {
            var books = await _repository.GetLowStockAsync(request.Threshold, cancellationToken);

            return books.Select(book => new BookDto(
                book.Id,
                book.Title,
                book.Author,
                book.Price,
                book.StockQuantity,
                book.Rating,
                book.Category
            )).ToList();
        }
    }
}

