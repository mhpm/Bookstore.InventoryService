using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        public static decimal CalculateDiscount(decimal originalPrice, decimal discountPercentage)
            => throw new NotSupportedException();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>()
                .Property(b => b.Price)
                .HasPrecision(18, 2);

            modelBuilder.HasDbFunction(typeof(InventoryDbContext).GetMethod(nameof(CalculateDiscount), new[] { typeof(decimal), typeof(decimal) })!)
                .HasName("fn_CalculateDiscount");
        }
    }
}
