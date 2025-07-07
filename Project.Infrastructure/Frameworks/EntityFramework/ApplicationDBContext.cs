using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Project.Infrastructure.Frameworks.Identity; // Si tienes ApplicationUser y ApplicationRole
using Project.Domain.Entities; // Si tienes otras entidades propias

namespace Project.Infrastructure.Frameworks.EntityFramework
{
    public class ApplicationDBContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }
        // DbSets for your entities
        public DbSet<Client> Clients { get; set; }
        public DbSet<Product> Products { get; set; }  
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        
        // DbSet para el historial de contraseñas
        public DbSet<UserPasswordHistory> UserPasswordHistories { get; set; }

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configurar la relación entre ApplicationUser y UserPasswordHistory
            modelBuilder.Entity<UserPasswordHistory>()
                .HasOne(ph => ph.User)
                .WithMany(u => u.PasswordHistories)
                .HasForeignKey(ph => ph.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Índice para mejorar el rendimiento
            modelBuilder.Entity<UserPasswordHistory>()
                .HasIndex(ph => new { ph.UserId, ph.CreatedAt });
        }
    }
}

