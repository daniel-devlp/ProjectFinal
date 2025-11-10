using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.Entities;

namespace Project.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
    public void Configure(EntityTypeBuilder<Product> builder)
      {
       builder.ToTable("Products");

  builder.HasKey(p => p.ProductId);
   builder.Property(p => p.ProductId)
  .ValueGeneratedOnAdd();

builder.Property(p => p.Code)
 .HasMaxLength(50)
   .IsRequired();

            builder.Property(p => p.Name)
      .HasMaxLength(200)
       .IsRequired();

       builder.Property(p => p.Description)
   .HasMaxLength(1000);

    // Configuración de propiedad decimal - SOLUCIÓN AL WARNING
   builder.Property(p => p.Price)
   .HasColumnType("decimal(18,2)")
  .IsRequired();

       builder.Property(p => p.Stock)
   .IsRequired();

builder.Property(p => p.IsActive)
      .IsRequired();

     builder.Property(p => p.ImageUri)
      .HasMaxLength(500);

            // Relaciones
   builder.HasMany(p => p.InvoiceDetails)
     .WithOne(d => d.Product)
    .HasForeignKey(d => d.ProductID)
   .OnDelete(DeleteBehavior.Restrict);

          // Índices
     builder.HasIndex(p => p.Code)
  .IsUnique()
.HasDatabaseName("IX_Products_Code");

       builder.HasIndex(p => p.Name)
 .HasDatabaseName("IX_Products_Name");

       builder.HasIndex(p => p.IsActive)
     .HasDatabaseName("IX_Products_IsActive");

    // Configuración futura para PostgreSQL (comentada)
     /*
       builder.ToTable("products");
       builder.Property(p => p.ProductId).HasColumnName("product_id");
        builder.Property(p => p.Code).HasColumnName("code");
            builder.Property(p => p.Name).HasColumnName("name");
   builder.Property(p => p.Description).HasColumnName("description");
     builder.Property(p => p.Price).HasColumnName("price");
    builder.Property(p => p.Stock).HasColumnName("stock");
         builder.Property(p => p.IsActive).HasColumnName("is_active");
        builder.Property(p => p.ImageUri).HasColumnName("image_uri");
 */
     }
  }
}