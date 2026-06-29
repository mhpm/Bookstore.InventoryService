using Xunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using InventoryService.Data;
using InventoryService.Data.Repositories;
using InventoryService.Features.Books.Commands;
using InventoryService.Features.Books.Queries;
using InventoryService.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace InventoryService.Tests
{
    /// <summary>
    /// CLASE DE PRUEBAS EDUCATIVAS:
    /// Demuestra cómo el desacoplamiento mediante SOLID nos permite realizar tanto pruebas de integración
    /// (usando un repositorio real con base de datos en memoria) como pruebas unitarias puras (usando Fakes).
    /// </summary>
    public class BookHandlersTests
    {
        // Método auxiliar para crear un DbContext real en memoria
        private InventoryDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new InventoryDbContext(options);
        }

        #region Pruebas de Integración (Usando BookRepository real sobre EF Core In-Memory)

        [Fact]
        public async Task CreateBookCommandHandler_Should_Add_Book_To_Database()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            // DEPENDENCY INVERSION PRINCIPLE (DIP):
            // Instanciamos el repositorio real y se lo inyectamos al Handler.
            var repository = new BookRepository(context);
            var handler = new CreateBookCommandHandler(repository);
            var command = new CreateBookCommand("El Hobbit", "J.R.R. Tolkien", 150.00m, 5, 4.8, "Fantasía");

            // Act
            var bookId = await handler.Handle(command, CancellationToken.None);

            // Assert
            bookId.Should().BeGreaterThan(0);
            var bookInDb = await context.Books.FindAsync(bookId);
            bookInDb.Should().NotBeNull();
            bookInDb!.Title.Should().Be("El Hobbit");
            bookInDb.Price.Should().Be(150.00m);
            bookInDb.Rating.Should().Be(4.8);
            bookInDb.Category.Should().Be("Fantasía");
        }

        [Fact]
        public async Task GetBookByIdQueryHandler_Should_Return_BookDto_If_Book_Exists()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var seedBook = new Book { Title = "1984", Author = "George Orwell", Price = 99.99m, StockQuantity = 10 };
            context.Books.Add(seedBook);
            await context.SaveChangesAsync();

            var repository = new BookRepository(context);
            var handler = new GetBookByIdQueryHandler(repository);
            var query = new GetBookByIdQuery(seedBook.Id);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result!.Title.Should().Be("1984");
        }

        [Fact]
        public async Task GetBooksQueryHandler_Should_Return_All_Books()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Books.AddRange(new List<Book>
            {
                new() { Title = "Libro A", Author = "Autor A", Price = 10m, StockQuantity = 5 },
                new() { Title = "Libro B", Author = "Autor B", Price = 20m, StockQuantity = 10 }
            });
            await context.SaveChangesAsync();

            var repository = new BookRepository(context);
            var handler = new GetBooksQueryHandler(repository);
            var query = new GetBooksQuery();

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Count.Should().Be(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task UpdateBookCommandHandler_Should_Update_Fields_In_Database()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var seedBook = new Book { Title = "Título Original", Author = "Autor Original", Price = 50.00m, StockQuantity = 10 };
            context.Books.Add(seedBook);
            await context.SaveChangesAsync();

            var repository = new BookRepository(context);
            var handler = new UpdateBookCommandHandler(repository);
            var command = new UpdateBookCommand(seedBook.Id, Price: 75.50m, StockQuantity: 15);

            // Act
            var isUpdated = await handler.Handle(command, CancellationToken.None);

            // Assert
            isUpdated.Should().BeTrue();
            var updatedBook = await context.Books.FindAsync(seedBook.Id);
            updatedBook!.Price.Should().Be(75.50m);
            updatedBook.StockQuantity.Should().Be(15);
        }

        [Fact]
        public async Task DeleteBookCommandHandler_Should_Remove_Book_From_Database()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var seedBook = new Book { Title = "A Eliminar", Author = "Anónimo", Price = 10m, StockQuantity = 1 };
            context.Books.Add(seedBook);
            await context.SaveChangesAsync();

            var repository = new BookRepository(context);
            var handler = new DeleteBookCommandHandler(repository);
            var command = new DeleteBookCommand(seedBook.Id);

            // Act
            var isDeleted = await handler.Handle(command, CancellationToken.None);

            // Assert
            isDeleted.Should().BeTrue();
            var bookInDb = await context.Books.FindAsync(seedBook.Id);
            bookInDb.Should().BeNull();
        }

        [Fact]
        public async Task GetLowStockBooksQueryHandler_Should_Return_Books_Below_Threshold()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            context.Books.AddRange(new List<Book>
            {
                new() { Title = "Bajo Stock", Author = "Autor 1", Price = 10m, StockQuantity = 2 },
                new() { Title = "Suficiente Stock", Author = "Autor 2", Price = 15m, StockQuantity = 20 }
            });
            await context.SaveChangesAsync();

            var repository = new BookRepository(context);
            var handler = new GetLowStockBooksQueryHandler(repository);
            var query = new GetLowStockBooksQuery(5);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().ContainSingle();
            result.First().Title.Should().Be("Bajo Stock");
        }

        #endregion

        #region Pruebas Unitarias Puras (Usando un Fake de IBookRepository)

        // LISKOV SUBSTITUTION PRINCIPLE (LSP) & DEPENDENCY INVERSION PRINCIPLE (DIP):
        // Como los Handlers dependen de IBookRepository, podemos pasarles un FakeBookRepository (LSP).
        // Esto permite probar GetDiscountedBooksQueryHandler sin fallar por la función customizada de base de datos
        // fn_CalculateDiscount, la cual no es compatible con el motor In-Memory de EF Core.
        
        [Fact]
        public async Task GetDiscountedBooksQueryHandler_Should_Calculate_Discount_Correctly()
        {
            // Arrange
            var fakeRepository = new FakeBookRepository();
            fakeRepository.Books.Add(new Book { Id = 1, Title = "Clean Code", Author = "Uncle Bob", Price = 100m, StockQuantity = 10 });

            var handler = new GetDiscountedBooksQueryHandler(fakeRepository);
            var query = new GetDiscountedBooksQuery(10m); // 10% descuento

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().ContainSingle();
            result.First().DiscountedPrice.Should().Be(90m); // 100 - (10% de 100)
        }

        [Fact]
        public async Task BookStockService_Should_Reduce_Stock_Successfully()
        {
            // Arrange
            var fakeRepository = new FakeBookRepository();
            var book = new Book { Id = 1, Title = "Domain Driven Design", Author = "Eric Evans", Price = 80m, StockQuantity = 15 };
            fakeRepository.Books.Add(book);

            var stockService = new BookStockService(fakeRepository, NullLogger<BookStockService>.Instance);
            var itemsToReduce = new List<(int BookId, int Quantity)> { (1, 5) };

            // Act
            await stockService.ReduceStockAsync(itemsToReduce, CancellationToken.None);

            // Assert
            book.StockQuantity.Should().Be(10); // 15 - 5
            fakeRepository.SaveChangesCalled.Should().BeTrue();
        }

        #endregion
    }

    /// <summary>
    /// IMPLEMENTACIÓN FAKE PARA TESTEO:
    /// Esta clase simula un repositorio de base de datos usando colecciones en memoria de C#.
    /// Demuestra el poder de la inversión de dependencia, permitiendo tests rápidos y sin dependencias externas.
    /// </summary>
    public class FakeBookRepository : IBookRepository
    {
        public List<Book> Books { get; } = new();
        public bool SaveChangesCalled { get; private set; }

        public Task<Book?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Books.FirstOrDefault(b => b.Id == id));
        }

        public Task<Book?> GetByIdNoTrackingAsync(int id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Books.FirstOrDefault(b => b.Id == id));
        }

        public Task<List<Book>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Books.ToList());
        }

        public Task<(List<Book> Items, int TotalCount)> GetPagedAsync(
            int? pageNumber, 
            int? pageSize, 
            string? category, 
            string? searchQuery, 
            CancellationToken cancellationToken = default)
        {
            var query = Books.AsEnumerable();

            if (!string.IsNullOrEmpty(category) && !category.Equals("Todas", System.StringComparison.OrdinalIgnoreCase))
            {
                var catLower = category.ToLower();
                if (catLower == "general")
                {
                    query = query.Where(b => string.IsNullOrEmpty(b.Category) || b.Category.ToLower() == "general");
                }
                else
                {
                    query = query.Where(b => b.Category?.ToLower() == catLower);
                }
            }

            if (!string.IsNullOrEmpty(searchQuery))
            {
                var searchLower = searchQuery.ToLower();
                query = query.Where(b => (b.Title?.ToLower().Contains(searchLower) ?? false) || (b.Author?.ToLower().Contains(searchLower) ?? false));
            }

            var list = query.ToList();
            var totalCount = list.Count;

            List<Book> items;
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                items = list
                    .Skip((pageNumber.Value - 1) * pageSize.Value)
                    .Take(pageSize.Value)
                    .ToList();
            }
            else
            {
                items = list;
            }

            return Task.FromResult((items, totalCount));
        }

        public Task<List<Book>> GetLowStockAsync(int threshold, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Books.Where(b => b.StockQuantity < threshold).ToList());
        }

        public Task<List<(Book Book, decimal DiscountedPrice)>> GetDiscountedAsync(decimal discountPercentage, CancellationToken cancellationToken = default)
        {
            // Simulación en memoria del cálculo que hace la DB
            var result = Books.Select(b => (b, b.Price - (b.Price * discountPercentage / 100))).ToList();
            return Task.FromResult(result);
        }

        public Task AddAsync(Book book, CancellationToken cancellationToken = default)
        {
            Books.Add(book);
            return Task.CompletedTask;
        }

        public void Update(Book book)
        {
            // No requiere acción en colecciones directas
        }

        public void Delete(Book book)
        {
            Books.Remove(book);
        }

        public Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCalled = true;
            return Task.FromResult(true);
        }
    }
}
