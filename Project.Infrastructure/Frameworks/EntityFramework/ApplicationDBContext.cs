using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project.Infrastructure.Frameworks.Identity;
using Project.Domain.Entities;
using Project.Infrastructure.Persistence.Configurations;

namespace Project.Infrastructure.Frameworks.EntityFramework
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }

        // DbSets for entities
        public DbSet<Client> Clients { get; set; }
        public DbSet<Product> Products { get; set; }  
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        
        // DbSet para el historial de contraseñas
        public DbSet<UserPasswordHistory> UserPasswordHistories { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Aplicar configuraciones de Clean Architecture
            modelBuilder.ApplyConfiguration(new ClientConfiguration());
            
            // Configurar la relación entre ApplicationUser y UserPasswordHistory
            modelBuilder.Entity<UserPasswordHistory>()
                .HasOne(ph => ph.User)
                .WithMany(u => u.PasswordHistories)
                .HasForeignKey(ph => ph.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Índice para mejorar el rendimiento
            modelBuilder.Entity<UserPasswordHistory>()
                .HasIndex(ph => new { ph.UserId, ph.CreatedAt });

            // Configuración futura para PostgreSQL (comentada)
            /*
            // Para PostgreSQL: configurar esquemas y nomenclatura snake_case
            modelBuilder.HasDefaultSchema("public");
            
            // Configurar nombres de tablas en snake_case para PostgreSQL
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.GetTableName().ToSnakeCase());
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(property.GetColumnName().ToSnakeCase());
                }
            }
            */
        }
    }
}

