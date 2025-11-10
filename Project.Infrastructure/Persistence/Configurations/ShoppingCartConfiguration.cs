using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.Entities;

namespace Project.Infrastructure.Persistence.Configurations
{
    public class ShoppingCartConfiguration : IEntityTypeConfiguration<ShoppingCart>
    {
     public void Configure(EntityTypeBuilder<ShoppingCart> builder)
        {
      builder.ToTable("ShoppingCart");

      builder.HasKey(sc => sc.CartId);
         builder.Property(sc => sc.CartId)
 .ValueGeneratedOnAdd();

          builder.Property(sc => sc.UserId)
     .HasMaxLength(450)
    .IsRequired();

 builder.Property(sc => sc.ProductId)
       .IsRequired();

 builder.Property(sc => sc.Quantity)
   .IsRequired();

       builder.Property(sc => sc.UnitPrice)
   .HasColumnType("decimal(18,2)")
      .IsRequired();

       builder.Property(sc => sc.Subtotal)
               .HasColumnType("decimal(18,2)")
   .IsRequired();

    builder.Property(sc => sc.DateAdded)
    .IsRequired();

      builder.Property(sc => sc.UpdatedAt);

     // Relaciones
 builder.HasOne(sc => sc.Product)
    .WithMany()
 .HasForeignKey(sc => sc.ProductId)
       .OnDelete(DeleteBehavior.Restrict);

    // Índices
      builder.HasIndex(sc => sc.UserId)
  .HasDatabaseName("IX_ShoppingCart_UserId");

    builder.HasIndex(sc => new { sc.UserId, sc.ProductId })
 .IsUnique()
           .HasDatabaseName("IX_ShoppingCart_User_Product");

   builder.HasIndex(sc => sc.DateAdded)
        .HasDatabaseName("IX_ShoppingCart_DateAdded");

     // Configuración futura para PostgreSQL (comentada)
  /*
 builder.ToTable("shopping_cart");
   builder.Property(sc => sc.CartId).HasColumnName("cart_id");
     builder.Property(sc => sc.UserId).HasColumnName("user_id");
      builder.Property(sc => sc.ProductId).HasColumnName("product_id");
      builder.Property(sc => sc.Quantity).HasColumnName("quantity");
 builder.Property(sc => sc.UnitPrice).HasColumnName("unit_price");
     builder.Property(sc => sc.Subtotal).HasColumnName("subtotal");
         builder.Property(sc => sc.DateAdded).HasColumnName("date_added");
      builder.Property(sc => sc.UpdatedAt).HasColumnName("updated_at");
   */
        }
    }
}