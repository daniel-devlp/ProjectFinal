using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.Entities;

namespace Project.Infrastructure.Persistence.Configurations
{
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {
      builder.ToTable("Invoices");

            builder.HasKey(i => i.InvoiceId);
            builder.Property(i => i.InvoiceId)
                .ValueGeneratedOnAdd();

   builder.Property(i => i.InvoiceNumber)
                .HasMaxLength(50)
    .IsRequired();

         builder.Property(i => i.ClientId)
    .IsRequired();

     builder.Property(i => i.UserId)
          .HasMaxLength(450)
       .IsRequired();

         builder.Property(i => i.IssueDate)
           .IsRequired();

     // Configuración de propiedades decimales - SOLUCIÓN AL WARNING
            builder.Property(i => i.Subtotal)
         .HasColumnType("decimal(18,2)")
        .IsRequired();

   builder.Property(i => i.Tax)
.HasColumnType("decimal(18,2)")
     .IsRequired();

    builder.Property(i => i.Total)
 .HasColumnType("decimal(18,2)")
 .IsRequired();

            builder.Property(i => i.Observations)
 .HasMaxLength(1000);

     // Relaciones
      builder.HasOne(i => i.Client)
                .WithMany(c => c.Invoices)
                .HasForeignKey(i => i.ClientId)
             .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(i => i.InvoiceDetails)
          .WithOne(d => d.Invoice)
          .HasForeignKey(d => d.InvoiceID)
           .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(i => i.InvoiceNumber)
  .IsUnique()
     .HasDatabaseName("IX_Invoices_InvoiceNumber");

            builder.HasIndex(i => i.ClientId)
          .HasDatabaseName("IX_Invoices_ClientId");

   builder.HasIndex(i => i.UserId)
       .HasDatabaseName("IX_Invoices_UserId");

       builder.HasIndex(i => i.IssueDate)
     .HasDatabaseName("IX_Invoices_IssueDate");

 // Configuración futura para PostgreSQL (comentada)
       /*
            builder.ToTable("invoices");
     builder.Property(i => i.InvoiceId).HasColumnName("invoice_id");
         builder.Property(i => i.InvoiceNumber).HasColumnName("invoice_number");
     builder.Property(i => i.ClientId).HasColumnName("client_id");
       builder.Property(i => i.UserId).HasColumnName("user_id");
            builder.Property(i => i.IssueDate).HasColumnName("issue_date");
            builder.Property(i => i.Subtotal).HasColumnName("subtotal");
          builder.Property(i => i.Tax).HasColumnName("tax");
 builder.Property(i => i.Total).HasColumnName("total");
          builder.Property(i => i.Observations).HasColumnName("observations");
         */
  }
    }
}