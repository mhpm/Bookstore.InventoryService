using MediatR;
using InventoryService.Data;

namespace InventoryService.Features.Books.Commands
{
    public record DeleteBookCommand(int Id) : IRequest<bool>;

    public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, bool>
    {
        private readonly InventoryDbContext _context;

        public DeleteBookCommandHandler(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
        {
            var book = await _context.Books.FindAsync([request.Id], cancellationToken);

            if (book == null)
            {
                return false;
            }

            _context.Books.Remove(book);
            
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
