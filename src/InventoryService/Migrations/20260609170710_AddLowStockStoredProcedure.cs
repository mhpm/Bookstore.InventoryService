using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryService.Migrations
{
    /// <inheritdoc />
    public partial class AddLowStockStoredProcedure : Migration
    {
        /// <inheritdoc />
         protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_GetLowStockBooks
                    @Threshold INT
                AS
                BEGIN
                    SELECT * FROM [Books]
                    WHERE [StockQuantity] <= @Threshold;
                END
            ");
        }

        /// <inheritdoc />
         protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP PROCEDURE sp_GetLowStockBooks;
            ");
        }
    }
}
