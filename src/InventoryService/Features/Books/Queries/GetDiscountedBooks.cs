using MediatR;
using Microsoft.EntityFrameworkCore;
using InventoryService.Data;

namespace InventoryService.Features.Books.Queries
{
    public record DiscountedBookDto(
        int Id,
        string Title,
        string Author,
        decimal Price,
        decimal DiscountedPrice,
        int StockQuantity
    );

    public record GetDiscountedBooksQuery(decimal DiscountPercentage) : IRequest<List<DiscountedBookDto>>;

    public class GetDiscountedBooksQueryHandler : IRequestHandler<GetDiscountedBooksQuery, List<DiscountedBookDto>>
    {
        private readonly InventoryDbContext _context;

        public GetDiscountedBooksQueryHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<DiscountedBookDto>> Handle(GetDiscountedBooksQuery request, CancellationToken cancellationToken)
        {
            return await _context.Books
                .Select(book => new DiscountedBookDto(
                    book.Id,
                    book.Title,
                    book.Author,
                    book.Price,
                    InventoryDbContext.CalculateDiscount(book.Price, request.DiscountPercentage),
                    book.StockQuantity
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
