using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Project.Domain.Entities;

namespace Project.Infrastructure.Persistence.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.ToTable("Clients");

            builder.HasKey(c => c.ClientId);
            builder.Property(c => c.ClientId)
                .ValueGeneratedOnAdd();

            // Configuración compatible con BD existente
            builder.Property(c => c.IdentificationType)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue("CEDULA"); // Valor por defecto para registros existentes

            builder.Property(c => c.IdentificationNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(c => c.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.LastName)
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(c => c.Phone)
                .HasMaxLength(20);

            builder.Property(c => c.Email)
                .HasMaxLength(250);

            builder.Property(c => c.Address)
                .HasMaxLength(500);

            // ? Configuración para borrado lógico
            builder.Property(c => c.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.UpdatedAt);

            builder.Property(c => c.DeletedAt); // ? Campo para auditoría de eliminación

            // ? Filtro global para borrado lógico
            builder.HasQueryFilter(c => c.IsActive);

            // Índices para mejorar rendimiento
            builder.HasIndex(c => c.IdentificationNumber)
                .IsUnique()
                .HasDatabaseName("IX_Clients_IdentificationNumber");

            builder.HasIndex(c => c.Email)
                .HasDatabaseName("IX_Clients_Email");

            builder.HasIndex(c => c.IsActive)
                .HasDatabaseName("IX_Clients_IsActive");

            // Relaciones
            builder.HasMany(c => c.Invoices)
                .WithOne(i => i.Client)
                .HasForeignKey(i => i.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración futura para PostgreSQL (comentada)
            /*
            builder.ToTable("clients");
            builder.Property(c => c.ClientId).HasColumnName("client_id");
            builder.Property(c => c.IdentificationType).HasColumnName("identification_type");
            builder.Property(c => c.IdentificationNumber).HasColumnName("identification_number");
            builder.Property(c => c.FirstName).HasColumnName("first_name");
            builder.Property(c => c.LastName).HasColumnName("last_name");
            builder.Property(c => c.Phone).HasColumnName("phone");
            builder.Property(c => c.Email).HasColumnName("email");
            builder.Property(c => c.Address).HasColumnName("address");
            builder.Property(c => c.IsActive).HasColumnName("is_active");
            builder.Property(c => c.CreatedAt).HasColumnName("created_at");
            builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");
            builder.Property(c => c.DeletedAt).HasColumnName("deleted_at");
            */
        }
    }
}