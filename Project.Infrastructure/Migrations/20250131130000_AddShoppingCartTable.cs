using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShoppingCartTable : Migration
    {
     /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
  name: "ShoppingCart",
      columns: table => new
       {
           CartId = table.Column<int>(type: "int", nullable: false)
           .Annotation("SqlServer:Identity", "1, 1"),
        UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
             ProductId = table.Column<int>(type: "int", nullable: false),
               Quantity = table.Column<int>(type: "int", nullable: false),
     UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
          Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
        DateAdded = table.Column<DateTime>(type: "datetime2", nullable: false),
      UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
    },
              constraints: table =>
         {
   table.PrimaryKey("PK_ShoppingCart", x => x.CartId);
       table.ForeignKey(
      name: "FK_ShoppingCart_Products_ProductId",
    column: x => x.ProductId,
                 principalTable: "Products",
            principalColumn: "ProductId",
      onDelete: ReferentialAction.Restrict);
  });

         migrationBuilder.CreateIndex(
   name: "IX_ShoppingCart_DateAdded",
table: "ShoppingCart",
        column: "DateAdded");

  migrationBuilder.CreateIndex(
          name: "IX_ShoppingCart_ProductId",
         table: "ShoppingCart",
          column: "ProductId");

            migrationBuilder.CreateIndex(
name: "IX_ShoppingCart_User_Product",
  table: "ShoppingCart",
   columns: new[] { "UserId", "ProductId" },
       unique: true);

         migrationBuilder.CreateIndex(
    name: "IX_ShoppingCart_UserId",
       table: "ShoppingCart",
            column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
       migrationBuilder.DropTable(
        name: "ShoppingCart");
}
  }
}