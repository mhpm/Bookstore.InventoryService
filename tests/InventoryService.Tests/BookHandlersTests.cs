using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using InventoryService.Data;
using InventoryService.Features.Books.Commands;
using InventoryService.Features.Books.Queries;

namespace InventoryService.Tests
{
    public class BookHandlersTests
    {
        // Método auxiliar para crear un DbContext en memoria limpio en cada prueba
        private InventoryDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new InventoryDbContext(options);
        }

        [Fact]
        public async Task CreateBookCommandHandler_Should_Add_Book_To_Database()
        {
            var context = GetInMemoryDbContext();
            var handler = new CreateBookCommandHandler(context);
            var command = new CreateBookCommand("El Hobbit", "J.R.R. Tolkien", 150.00m, 5);

            var bookId = await handler.Handle(command, CancellationToken.None);

            bookId.Should().BeGreaterThan(0);

            var bookInDb = await context.Books.FindAsync(bookId);
            bookInDb.Should().NotBeNull();
            bookInDb!.Title.Should().Be("El Hobbit");
            bookInDb.Author.Should().Be("J.R.R. Tolkien");
            bookInDb.Price.Should().Be(150.00m);
            bookInDb.StockQuantity.Should().Be(5);
        }

        [Fact]
        public async Task GetBookByIdQueryHandler_Should_Return_BookDto_If_Book_Exists()
        {
            var context = GetInMemoryDbContext();
            
            var seedBook = new Book { Title = "1984", Author = "George Orwell", Price = 99.99m, StockQuantity = 10 };
            context.Books.Add(seedBook);
            await context.SaveChangesAsync();

            var handler = new GetBookByIdQueryHandler(context);
            var query = new GetBookByIdQuery(seedBook.Id);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Title.Should().Be("1984");
            result.Author.Should().Be("George Orwell");
            result.Price.Should().Be(99.99m);
            result.StockQuantity.Should().Be(10);
        }

        [Fact]
        public async Task GetBookByIdQueryHandler_Should_Return_Null_If_Book_Does_Not_Exist()
        {
            var context = GetInMemoryDbContext();
            var handler = new GetBookByIdQueryHandler(context);
            var query = new GetBookByIdQuery(999);
            var result = await handler.Handle(query, CancellationToken.None);

            result.Should().BeNull();
        }

        [Fact]
        public async Task GetBooksQueryHandler_Should_Return_All_Books()
        {
            var context = GetInMemoryDbContext();
            context.Books.AddRange(new List<Book>
            {
                new() { Title = "Libro A", Author = "Autor A", Price = 10m, StockQuantity = 5 },
                new() { Title = "Libro B", Author = "Autor B", Price = 20m, StockQuantity = 10 }
            });
            await context.SaveChangesAsync();

            var handler = new GetBooksQueryHandler(context);
            var query = new GetBooksQuery();

            var result = await handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Count.Should().Be(2);
            result.Should().Contain(b => b.Title == "Libro A");
            result.Should().Contain(b => b.Title == "Libro B");
        }

        [Fact]
        public async Task UpdateBookCommandHandler_Should_Update_Fields_In_Database()
        {
            var context = GetInMemoryDbContext();
            var seedBook = new Book { Title = "Título Original", Author = "Autor Original", Price = 50.00m, StockQuantity = 10 };
            context.Books.Add(seedBook);
            await context.SaveChangesAsync();

            var handler = new UpdateBookCommandHandler(context);

            // Queremos actualizar solo el precio y el stock usando nuestra lógica de Mapster parcial
            var command = new UpdateBookCommand(seedBook.Id, Price: 75.50m, StockQuantity: 15);

            var isUpdated = await handler.Handle(command, CancellationToken.None);

            isUpdated.Should().BeTrue();
            var updatedBook = await context.Books.FindAsync(seedBook.Id);
            updatedBook!.Price.Should().Be(75.50m);
            updatedBook.StockQuantity.Should().Be(15);
            updatedBook.Title.Should().Be("Título Original");
            updatedBook.Author.Should().Be("Autor Original");
        }

        [Fact]
        public async Task DeleteBookCommandHandler_Should_Remove_Book_From_Database()
        {
            var context = GetInMemoryDbContext();
            var seedBook = new Book { Title = "A Eliminar", Author = "Anónimo", Price = 10m, StockQuantity = 1 };
            context.Books.Add(seedBook);
            await context.SaveChangesAsync();

            var handler = new DeleteBookCommandHandler(context);
            var command = new DeleteBookCommand(seedBook.Id);

            var isDeleted = await handler.Handle(command, CancellationToken.None);

            isDeleted.Should().BeTrue();
            var bookInDb = await context.Books.FindAsync(seedBook.Id);
            bookInDb.Should().BeNull();
        }
    }
}
