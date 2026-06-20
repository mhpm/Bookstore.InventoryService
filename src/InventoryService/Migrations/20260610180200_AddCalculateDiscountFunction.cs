using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryService.Migrations
{
    /// <inheritdoc />
    public partial class AddCalculateDiscountFunction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
             migrationBuilder.Sql(@"
                CREATE FUNCTION fn_CalculateDiscount
                (
                    @OriginalPrice DECIMAL(18,2),
                    @DiscountPercentage DECIMAL(5,2)
                )
                RETURNS DECIMAL(18,2)
                AS
                BEGIN
                    RETURN @OriginalPrice - (@OriginalPrice * (@DiscountPercentage / 100.00));
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP FUNCTION fn_CalculateDiscount;
            ");
        }
    }
}
