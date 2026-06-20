namespace InventoryService.Data
{
    public class Book
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
    }
}
