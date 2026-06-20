using MediatR;
using Microsoft.AspNetCore.Mvc;
using InventoryService.Features.Books.Commands;
using InventoryService.Features.Books.Queries;

namespace InventoryService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BooksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Endpoint POST: api/books
        [HttpPost]
        public async Task<IActionResult> CreateBook([FromBody] CreateBookCommand command)
        {
            if (command == null)
            {
                return BadRequest("El comando no puede ser nulo.");
            }

            var bookId = await _mediator.Send(command);

            return CreatedAtAction(nameof(CreateBook), new { id = bookId }, bookId);
        }

        [HttpGet]
        public async Task<ActionResult<List<BookDto>>> GetBooks()
        {
            var books = await _mediator.Send(new GetBooksQuery());
            return Ok(books);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBookById(int id)
        {
            var book = await _mediator.Send(new GetBookByIdQuery(id));

            if (book == null)
            {
                return NotFound($"No se encontró ningún libro con el ID {id}.");
            }

            return Ok(book);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, [FromBody] UpdateBookCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("El ID de la URL no coincide con el ID del cuerpo.");
            }

            var updated = await _mediator.Send(command);

            if (!updated)
            {
                return NotFound($"No se encontró ningún libro con el ID {id} para actualizar.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var deleted = await _mediator.Send(new DeleteBookCommand(id));

            if (!deleted)
            {
                return NotFound($"No se encontró ningún libro con el ID {id} para eliminar.");
            }

            return NoContent();
        }

        [HttpGet("low-stock/{threshold}")]
        public async Task<ActionResult<List<BookDto>>> GetLowStockBooks(int threshold)
        {
            var books = await _mediator.Send(new GetLowStockBooksQuery(threshold));
            return Ok(books);
        }

        [HttpGet("discounts/{percentage}")]
        public async Task<ActionResult<List<DiscountedBookDto>>> GetDiscountedBooks(decimal percentage)
        {
            var books = await _mediator.Send(new GetDiscountedBooksQuery(percentage));
            return Ok(books);
        }

        [HttpGet("test-error")]
        public IActionResult TestError()
        {
            throw new Exception("¡Esta es una excepción simulada de prueba para verificar nuestro Middleware!");
        }
    }
}
