using MediatR;
using Microsoft.EntityFrameworkCore;
using InventoryService.Data;

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

    public class GetBooksQueryHandler : IRequestHandler<GetBooksQuery, List<BookDto>>
    {
        private readonly InventoryDbContext _context;

        public GetBooksQueryHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<List<BookDto>> Handle(GetBooksQuery request, CancellationToken cancellationToken)
        {
            return await _context.Books
                .Select(book => new BookDto(
                    book.Id,
                    book.Title,
                    book.Author,
                    book.Price,
                    book.StockQuantity
                ))
                .ToListAsync(cancellationToken);
        }
    }
}
