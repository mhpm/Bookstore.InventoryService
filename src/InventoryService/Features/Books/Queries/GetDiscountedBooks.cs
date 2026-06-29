using MediatR;
using InventoryService.Data.Repositories;

namespace InventoryService.Features.Books.Queries
{
    public record DiscountedBookDto(
        int Id,
        string Title,
        string Author,
        decimal Price,
        decimal DiscountedPrice,
        int StockQuantity,
        double Rating,
        string Category
    );

    public record GetDiscountedBooksQuery(decimal DiscountPercentage) : IRequest<List<DiscountedBookDto>>;

    /// <summary>
    /// DEPENDENCY INVERSION PRINCIPLE (DIP):
    /// El Handler consume IBookRepository en lugar del DbContext directamente.
    /// 
    /// CQRS PATTERN (QUERY) & OPEN/CLOSED PRINCIPLE (OCP):
    /// Esta consulta calcula los precios con descuento delegando al repositorio. La invocación a
    /// la función de base de datos 'CalculateDiscount' está oculta en la capa de datos.
    /// Si cambia la lógica física del cálculo, el Handler permanece intacto y cerrado al cambio.
    /// </summary>
    public class GetDiscountedBooksQueryHandler : IRequestHandler<GetDiscountedBooksQuery, List<DiscountedBookDto>>
    {
        private readonly IBookRepository _repository;

        public GetDiscountedBooksQueryHandler(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<DiscountedBookDto>> Handle(GetDiscountedBooksQuery request, CancellationToken cancellationToken)
        {
            var results = await _repository.GetDiscountedAsync(request.DiscountPercentage, cancellationToken);

            return results.Select(r => new DiscountedBookDto(
                r.Book.Id,
                r.Book.Title,
                r.Book.Author,
                r.Book.Price,
                r.DiscountedPrice,
                r.Book.StockQuantity,
                r.Book.Rating,
                r.Book.Category
            )).ToList();
        }
    }
}

