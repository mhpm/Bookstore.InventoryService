using MediatR;
using InventoryService.Data;
using InventoryService.Data.Repositories;

namespace InventoryService.Features.Books.Commands
{
    public record DeleteBookCommand(int Id) : IRequest<bool>;

    /// <summary>
    /// DEPENDENCY INVERSION PRINCIPLE (DIP):
    /// El Handler de borrado depende únicamente de IBookRepository, eliminando el acoplamiento a la base de datos física.
    /// 
    /// CQRS PATTERN (COMMAND):
    /// Este es un Comando de escritura. Recupera la entidad, solicita su eliminación a través del repositorio y
    /// confirma los cambios en la transacción.
    /// </summary>
    public class DeleteBookCommandHandler : IRequestHandler<DeleteBookCommand, bool>
    {
        private readonly IBookRepository _repository;

        public DeleteBookCommandHandler(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
        {
            var book = await _repository.GetByIdAsync(request.Id, cancellationToken);

            if (book == null)
            {
                return false;
            }

            _repository.Delete(book);
            
            await _repository.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}

