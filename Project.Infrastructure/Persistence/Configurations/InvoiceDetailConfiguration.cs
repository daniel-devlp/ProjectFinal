using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.Entities;

namespace Project.Infrastructure.Persistence.Configurations
{
    public class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>
    {
        public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
        {
     builder.ToTable("InvoiceDetails");

            builder.HasKey(d => d.InvoiceDetailId);
      builder.Property(d => d.InvoiceDetailId)
      .ValueGeneratedOnAdd();

   builder.Property(d => d.InvoiceID)
      .IsRequired();

            builder.Property(d => d.ProductID)
   .IsRequired();

 builder.Property(d => d.Quantity)
      .IsRequired();

        // Configuración de propiedades decimales - SOLUCIÓN AL WARNING
            builder.Property(d => d.UnitPrice)
      .HasColumnType("decimal(18,2)")
     .IsRequired();

            builder.Property(d => d.Subtotal)
         .HasColumnType("decimal(18,2)")
          .IsRequired();

    // Relaciones
        builder.HasOne(d => d.Invoice)
   .WithMany(i => i.InvoiceDetails)
 .HasForeignKey(d => d.InvoiceID)
     .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Product)
            .WithMany(p => p.InvoiceDetails)
                .HasForeignKey(d => d.ProductID)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices
         builder.HasIndex(d => d.InvoiceID)
    .HasDatabaseName("IX_InvoiceDetails_InvoiceID");

    builder.HasIndex(d => d.ProductID)
    .HasDatabaseName("IX_InvoiceDetails_ProductID");

            // Configuración futura para PostgreSQL (comentada)
      /*
   builder.ToTable("invoice_details");
   builder.Property(d => d.InvoiceDetailId).HasColumnName("invoice_detail_id");
            builder.Property(d => d.InvoiceID).HasColumnName("invoice_id");
 builder.Property(d => d.ProductID).HasColumnName("product_id");
        builder.Property(d => d.Quantity).HasColumnName("quantity");
    builder.Property(d => d.UnitPrice).HasColumnName("unit_price");
builder.Property(d => d.Subtotal).HasColumnName("subtotal");
          */
 }
    }
}