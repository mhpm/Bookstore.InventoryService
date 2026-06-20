using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
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
            var thresholdParam = new SqlParameter("@Threshold", request.Threshold);

            var books = await _context.Books
                .FromSqlRaw("EXEC sp_GetLowStockBooks @Threshold", thresholdParam)
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
