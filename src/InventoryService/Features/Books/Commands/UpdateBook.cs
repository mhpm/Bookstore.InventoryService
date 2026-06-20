using MediatR;
using InventoryService.Data;
using Mapster;

namespace InventoryService.Features.Books.Commands
{
    public record UpdateBookCommand(
        int Id,
        string? Title = null,
        string? Author = null,
        decimal? Price = null,
        int? StockQuantity = null
    ) : IRequest<bool>;

    public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, bool>
    {
        private readonly InventoryDbContext _context;

        public UpdateBookCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
        {
            var book = await _context.Books.FindAsync(new object[] { request.Id }, cancellationToken);

            if (book == null)
            {
                return false; 
            }

            var config = new TypeAdapterConfig();
            
            config.NewConfig<UpdateBookCommand, Book>()
                  .IgnoreNullValues(true);

            request.Adapt(book, config);

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
