using MediatR;
using InventoryService.Data;

namespace InventoryService.Features.Books.Commands
{
    public record CreateBookCommand(
        string Title, 
        string Author, 
        decimal Price, 
        int StockQuantity
    ) : IRequest<int>;

    public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, int>
    {
        private readonly InventoryDbContext _context;

        public CreateBookCommandHandler(InventoryDbContext context)
        {
            _context = context;
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

            _context.Books.Add(book);
            await _context.SaveChangesAsync(cancellationToken);

            return book.Id;
        }
    }
}
