using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixDecimalPropertiesAndConfiguration : Migration
    {
   /// <inheritdoc />
 protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Actualizar tipos de datos decimales para Invoice
     migrationBuilder.AlterColumn<decimal>(
  name: "Total",
         table: "Invoices",
type: "decimal(18,2)",
       nullable: false,
 oldClrType: typeof(decimal),
      oldType: "decimal(18,2)",
       oldNullable: false);

         migrationBuilder.AlterColumn<decimal>(
  name: "Tax",
  table: "Invoices",
  type: "decimal(18,2)",
    nullable: false,
       oldClrType: typeof(decimal),
   oldType: "decimal(18,2)",
         oldNullable: false);

      migrationBuilder.AlterColumn<decimal>(
      name: "Subtotal",
   table: "Invoices",
         type: "decimal(18,2)",
     nullable: false,
   oldClrType: typeof(decimal),
    oldType: "decimal(18,2)",
      oldNullable: false);

    // Actualizar tipos de datos decimales para InvoiceDetail
     migrationBuilder.AlterColumn<decimal>(
 name: "UnitPrice",
          table: "InvoiceDetails",
 type: "decimal(18,2)",
   nullable: false,
     oldClrType: typeof(decimal),
  oldType: "decimal(18,2)",
     oldNullable: false);

      migrationBuilder.AlterColumn<decimal>(
     name: "Subtotal",
   table: "InvoiceDetails",
     type: "decimal(18,2)",
  nullable: false,
     oldClrType: typeof(decimal),
  oldType: "decimal(18,2)",
       oldNullable: false);

    // Actualizar tipos de datos decimales para Product
         migrationBuilder.AlterColumn<decimal>(
    name: "Price",
     table: "Products",
       type: "decimal(18,2)",
         nullable: false,
    oldClrType: typeof(decimal),
  oldType: "decimal(18,2)",
    oldNullable: false);

       // Crear índices adicionales para mejor performance
        migrationBuilder.CreateIndex(
     name: "IX_Invoices_InvoiceNumber",
     table: "Invoices",
    column: "InvoiceNumber",
unique: true);

       migrationBuilder.CreateIndex(
name: "IX_Invoices_IssueDate",
      table: "Invoices",
   column: "IssueDate");

  migrationBuilder.CreateIndex(
      name: "IX_Products_Code",
   table: "Products",
     column: "Code",
       unique: true);

    migrationBuilder.CreateIndex(
     name: "IX_Products_Name",
      table: "Products",
 column: "Name");

      migrationBuilder.CreateIndex(
 name: "IX_Products_IsActive",
    table: "Products",
  column: "IsActive");
      }

        /// <inheritdoc />
     protected override void Down(MigrationBuilder migrationBuilder)
        {
       // Eliminar índices
     migrationBuilder.DropIndex(
      name: "IX_Invoices_InvoiceNumber",
   table: "Invoices");

     migrationBuilder.DropIndex(
 name: "IX_Invoices_IssueDate",
    table: "Invoices");

          migrationBuilder.DropIndex(
     name: "IX_Products_Code",
  table: "Products");

     migrationBuilder.DropIndex(
    name: "IX_Products_Name",
       table: "Products");

    migrationBuilder.DropIndex(
   name: "IX_Products_IsActive",
     table: "Products");

     // Revertir cambios de tipos de datos (aunque no es necesario ya que son iguales)
        }
    }
}