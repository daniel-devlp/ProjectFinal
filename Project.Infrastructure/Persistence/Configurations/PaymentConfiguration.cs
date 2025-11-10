// CONFIGURACIONES EF PARA MÓDULO DE PAGOS - PREPARADO PARA IMPLEMENTACIÓN FUTURA
// Descomentar cuando sea necesario implementar sistema de pagos

/*
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.Entities;

namespace Project.Infrastructure.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
    public void Configure(EntityTypeBuilder<Payment> builder)
      {
          builder.ToTable("Payments");

builder.HasKey(p => p.PaymentId);
      builder.Property(p => p.PaymentId)
    .ValueGeneratedOnAdd();

            builder.Property(p => p.InvoiceId)
    .IsRequired();

     builder.Property(p => p.PaymentMethodId)
   .HasMaxLength(50)
            .IsRequired();

            builder.Property(p => p.Amount)
   .HasColumnType("decimal(18,2)")
   .IsRequired();

        builder.Property(p => p.TransactionId)
          .HasMaxLength(100)
         .IsRequired();

        builder.Property(p => p.Status)
     .HasConversion<int>()
       .IsRequired();

            builder.Property(p => p.PaymentDate)
            .IsRequired();

 builder.Property(p => p.ProcessedAt);

   builder.Property(p => p.ProcessorResponse)
.HasMaxLength(1000);

  builder.Property(p => p.FailureReason)
             .HasMaxLength(500);

    // Relaciones
            builder.HasOne(p => p.Invoice)
                .WithMany()
    .HasForeignKey(p => p.InvoiceId)
      .OnDelete(DeleteBehavior.Restrict);

builder.HasOne(p => p.PaymentMethod)
    .WithMany(pm => pm.Payments)
                .HasForeignKey(p => p.PaymentMethodId)
             .OnDelete(DeleteBehavior.Restrict);

            // Índices
  builder.HasIndex(p => p.InvoiceId)
     .HasDatabaseName("IX_Payments_InvoiceId");

     builder.HasIndex(p => p.TransactionId)
         .IsUnique()
           .HasDatabaseName("IX_Payments_TransactionId");

            builder.HasIndex(p => p.Status)
        .HasDatabaseName("IX_Payments_Status");

  builder.HasIndex(p => p.PaymentDate)
     .HasDatabaseName("IX_Payments_PaymentDate");

       // Configuración futura para PostgreSQL (comentada)
       /*
       builder.ToTable("payments");
      builder.Property(p => p.PaymentId).HasColumnName("payment_id");
   builder.Property(p => p.InvoiceId).HasColumnName("invoice_id");
 builder.Property(p => p.PaymentMethodId).HasColumnName("payment_method_id");
  builder.Property(p => p.Amount).HasColumnName("amount");
 builder.Property(p => p.TransactionId).HasColumnName("transaction_id");
        builder.Property(p => p.Status).HasColumnName("status");
  builder.Property(p => p.PaymentDate).HasColumnName("payment_date");
      builder.Property(p => p.ProcessedAt).HasColumnName("processed_at");
            builder.Property(p => p.ProcessorResponse).HasColumnName("processor_response");
         builder.Property(p => p.FailureReason).HasColumnName("failure_reason");
*//*
        }
    }

    public class PaymentMethodConfiguration : IEntityTypeConfiguration<PaymentMethod>
    {
        public void Configure(EntityTypeBuilder<PaymentMethod> builder)
        {
    builder.ToTable("PaymentMethods");

            builder.HasKey(pm => pm.PaymentMethodId);
            
      builder.Property(pm => pm.PaymentMethodId)
       .HasMaxLength(50)
           .IsRequired();

            builder.Property(pm => pm.Name)
          .HasMaxLength(100)
          .IsRequired();

            builder.Property(pm => pm.Description)
                .HasMaxLength(500);

            builder.Property(pm => pm.IsActive)
     .IsRequired();

            builder.Property(pm => pm.Type)
                .HasConversion<int>()
 .IsRequired();

            builder.Property(pm => pm.ProcessorConfig)
              .HasMaxLength(2000);

    builder.Property(pm => pm.MinAmount)
       .HasColumnType("decimal(18,2)");

     builder.Property(pm => pm.MaxAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(pm => pm.ProcessingFee)
         .HasColumnType("decimal(18,2)");

      // Relaciones
     builder.HasMany(pm => pm.Payments)
                .WithOne(p => p.PaymentMethod)
          .HasForeignKey(p => p.PaymentMethodId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
         builder.HasIndex(pm => pm.IsActive)
    .HasDatabaseName("IX_PaymentMethods_IsActive");

            builder.HasIndex(pm => pm.Type)
      .HasDatabaseName("IX_PaymentMethods_Type");

     // Datos semilla
     builder.HasData(
new PaymentMethod("CASH", "Efectivo", PaymentType.Cash) { Description = "Pago en efectivo", ProcessingFee = 0 },
 new PaymentMethod("CREDIT_CARD", "Tarjeta de Crédito", PaymentType.CreditCard) { Description = "Pago con tarjeta de crédito", ProcessingFee = 0.03m },
     new PaymentMethod("DEBIT_CARD", "Tarjeta de Débito", PaymentType.DebitCard) { Description = "Pago con tarjeta de débito", ProcessingFee = 0.02m },
        new PaymentMethod("BANK_TRANSFER", "Transferencia Bancaria", PaymentType.BankTransfer) { Description = "Transferencia bancaria", ProcessingFee = 0.01m }
            );

  // Configuración futura para PostgreSQL (comentada)
       /*
 builder.ToTable("payment_methods");
      builder.Property(pm => pm.PaymentMethodId).HasColumnName("payment_method_id");
          builder.Property(pm => pm.Name).HasColumnName("name");
            builder.Property(pm => pm.Description).HasColumnName("description");
            builder.Property(pm => pm.IsActive).HasColumnName("is_active");
     builder.Property(pm => pm.Type).HasColumnName("type");
       builder.Property(pm => pm.ProcessorConfig).HasColumnName("processor_config");
            builder.Property(pm => pm.MinAmount).HasColumnName("min_amount");
       builder.Property(pm => pm.MaxAmount).HasColumnName("max_amount");
     builder.Property(pm => pm.ProcessingFee).HasColumnName("processing_fee");
            *//*
        }
    }
}
*/