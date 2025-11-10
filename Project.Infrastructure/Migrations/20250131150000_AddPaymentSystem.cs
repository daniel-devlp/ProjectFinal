using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Project.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentSystem : Migration
    {
      /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
        {
      // Crear tabla PaymentMethods
   migrationBuilder.CreateTable(
    name: "PaymentMethods",
       columns: table => new
      {
                PaymentMethodId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
           Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
  IsActive = table.Column<bool>(type: "bit", nullable: false),
      Type = table.Column<int>(type: "int", nullable: false),
      ProcessorConfig = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
     MinAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
      MaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
    ProcessingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
    IconUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
        DisplayOrder = table.Column<int>(type: "int", nullable: false)
      },
       constraints: table =>
   {
  table.PrimaryKey("PK_PaymentMethods", x => x.PaymentMethodId);
        });

    // Crear tabla Payments
     migrationBuilder.CreateTable(
   name: "Payments",
          columns: table => new
         {
 PaymentId = table.Column<int>(type: "int", nullable: false)
      .Annotation("SqlServer:Identity", "1, 1"),
         InvoiceId = table.Column<int>(type: "int", nullable: false),
    PaymentMethodId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
  Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
  TransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
         Status = table.Column<int>(type: "int", nullable: false),
PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
         ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
      ProcessorResponse = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
   FailureReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
     },
        constraints: table =>
  {
            table.PrimaryKey("PK_Payments", x => x.PaymentId);
    table.ForeignKey(
     name: "FK_Payments_Invoices_InvoiceId",
      column: x => x.InvoiceId,
     principalTable: "Invoices",
     principalColumn: "InvoiceId",
   onDelete: ReferentialAction.Restrict);
    table.ForeignKey(
 name: "FK_Payments_PaymentMethods_PaymentMethodId",
     column: x => x.PaymentMethodId,
      principalTable: "PaymentMethods",
  principalColumn: "PaymentMethodId",
   onDelete: ReferentialAction.Restrict);
    });

         // Crear índices para PaymentMethods
            migrationBuilder.CreateIndex(
    name: "IX_PaymentMethods_IsActive",
        table: "PaymentMethods",
       column: "IsActive");

      migrationBuilder.CreateIndex(
name: "IX_PaymentMethods_Type",
      table: "PaymentMethods",
   column: "Type");

   migrationBuilder.CreateIndex(
     name: "IX_PaymentMethods_DisplayOrder",
        table: "PaymentMethods",
      column: "DisplayOrder");

   // Crear índices para Payments
     migrationBuilder.CreateIndex(
       name: "IX_Payments_InvoiceId",
       table: "Payments",
     column: "InvoiceId");

      migrationBuilder.CreateIndex(
                name: "IX_Payments_TransactionId",
table: "Payments",
  column: "TransactionId",
   unique: true);

      migrationBuilder.CreateIndex(
 name: "IX_Payments_Status",
       table: "Payments",
           column: "Status");

            migrationBuilder.CreateIndex(
    name: "IX_Payments_PaymentDate",
 table: "Payments",
     column: "PaymentDate");

      migrationBuilder.CreateIndex(
   name: "IX_Payments_PaymentMethodId",
 table: "Payments",
column: "PaymentMethodId");

         // Insertar datos semilla para métodos de pago
     migrationBuilder.InsertData(
            table: "PaymentMethods",
        columns: new[] { "PaymentMethodId", "Name", "Description", "IsActive", "Type", "ProcessingFee", "MinAmount", "MaxAmount", "DisplayOrder", "IconUrl" },
           values: new object[,]
    {
  { "CASH", "Efectivo", "Pago en efectivo al recibir", true, 0, 0.00m, 0.00m, 99999.99m, 1, "https://res.cloudinary.com/dvdzabq8x/image/upload/v1699000000/payment-icons/cash.png" },
      { "CREDIT_CARD", "Tarjeta de Crédito", "Pago con tarjeta de crédito", true, 1, 0.03m, 0.01m, 99999.99m, 2, "https://res.cloudinary.com/dvdzabq8x/image/upload/v1699000000/payment-icons/credit-card.png" },
       { "DEBIT_CARD", "Tarjeta de Débito", "Pago con tarjeta de débito", true, 2, 0.02m, 0.01m, 99999.99m, 3, "https://res.cloudinary.com/dvdzabq8x/image/upload/v1699000000/payment-icons/debit-card.png" },
  { "BANK_TRANSFER", "Transferencia Bancaria", "Transferencia bancaria", true, 3, 0.01m, 0.01m, 99999.99m, 4, "https://res.cloudinary.com/dvdzabq8x/image/upload/v1699000000/payment-icons/bank-transfer.png" },
      { "MOBILE_MONEY", "Dinero Móvil", "Pago con dinero móvil", true, 6, 0.02m, 0.01m, 99999.99m, 5, "https://res.cloudinary.com/dvdzabq8x/image/upload/v1699000000/payment-icons/mobile-money.png" }
    });
        }

    /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
     {
          migrationBuilder.DropTable(
         name: "Payments");

            migrationBuilder.DropTable(
     name: "PaymentMethods");
  }
    }
}