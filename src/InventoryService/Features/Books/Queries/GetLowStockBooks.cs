using MediatR;
using Microsoft.EntityFrameworkCore;
using InventoryService.Data;

namespace InventoryService.Features.Books.Queries
{
    public record GetLowStockBooksQuery(int Threshold) : IRequest<List<BookDto>>;

    public class GetLowStockBooksQueryHandler : IRequestHandler<GetLowStockBooksQuery, List<BookDto>>
    {
        private readonly InventoryDbContext _context;

        public GetLowStockBooksQueryHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<BookDto>> Handle(GetLowStockBooksQuery request, CancellationToken cancellationToken)
        {
            var books = await _context.Books
                .Where(book => book.StockQuantity < request.Threshold)
                .ToListAsync(cancellationToken);

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
