using MediatR;
using InventoryService.Data;

namespace InventoryService.Features.Books.Queries
{
    public record GetBookByIdQuery(int Id) : IRequest<BookDto?>;

    public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDto?>
    {
        private readonly InventoryDbContext _context;

        public GetBookByIdQueryHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<BookDto?> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
        {
            var book = await _context.Books.FindAsync([request.Id], cancellationToken);

            if (book == null)
            {
                return null;
            }

            // Proyectamos la entidad al DTO
            return new BookDto(
                book.Id,
                book.Title,
                book.Author,
                book.Price,
                book.StockQuantity
            );
        }
    }
}
