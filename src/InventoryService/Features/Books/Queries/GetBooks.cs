using MediatR;
using InventoryService.Data.Repositories;

namespace InventoryService.Features.Books.Queries
{

    public record BookDto(
        int Id, 
        string Title, 
        string Author, 
        decimal Price, 
        int StockQuantity,
        double Rating,
        string Category
    );

    public record PaginatedBooksResponse(
        List<BookDto> Items,
        int PageNumber,
        int PageSize,
        int TotalCount,
        int TotalPages
    );

    public record GetBooksQuery(
        int? PageNumber = null, 
        int? PageSize = null, 
        string? Category = null, 
        string? SearchQuery = null
    ) : IRequest<PaginatedBooksResponse>;

    /// <summary>
    /// DEPENDENCY INVERSION PRINCIPLE (DIP):
    /// Depende de IBookRepository para obtener la lista de libros en lugar de EF Core directamente.
    /// 
    /// CQRS PATTERN (QUERY):
    /// Representa una Consulta de lectura (Query). No altera el estado del sistema. En el repositorio
    /// se optimiza usando AsNoTracking() para evitar sobrecarga de rendimiento por seguimiento de cambios.
    /// </summary>
    public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, PaginatedBooksResponse>
    {
        private readonly IBookRepository _repository;

        public GetBooksQueryHandler(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<PaginatedBooksResponse> Handle(GetBooksQuery request, CancellationToken cancellationToken)
        {
            var (books, totalCount) = await _repository.GetPagedAsync(
                request.PageNumber,
                request.PageSize,
                request.Category,
                request.SearchQuery,
                cancellationToken
            );

            var dtos = books.Select(book => new BookDto(
                book.Id,
                book.Title,
                book.Author,
                book.Price,
                book.StockQuantity,
                book.Rating,
                book.Category
            )).ToList();

            int pageNumber = request.PageNumber ?? 1;
            int pageSize = request.PageSize ?? totalCount;
            if (pageSize <= 0) pageSize = 10;
            int totalPages = (int)System.Math.Ceiling((double)totalCount / pageSize);

            return new PaginatedBooksResponse(dtos, pageNumber, pageSize, totalCount, totalPages);
        }
    }
}

