using MediatR;
using InventoryService.Data.Repositories;

namespace InventoryService.Features.Books.Queries
{
    public record GetBookByIdQuery(int Id) : IRequest<BookDto?>;

    /// <summary>
    /// DEPENDENCY INVERSION PRINCIPLE (DIP):
    /// El handler consume IBookRepository eliminando el acoplamiento directo a la tecnología de base de datos.
    /// 
    /// CQRS PATTERN (QUERY):
    /// Representa una Consulta de lectura por ID. Usa GetByIdNoTrackingAsync del repositorio para que la entidad
    /// no sea rastreada, reduciendo el consumo de memoria y optimizando la velocidad de respuesta.
    /// </summary>
    public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookDto?>
    {
        private readonly IBookRepository _repository;

        public GetBookByIdQueryHandler(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<BookDto?> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
        {
            var book = await _repository.GetByIdNoTrackingAsync(request.Id, cancellationToken);

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

