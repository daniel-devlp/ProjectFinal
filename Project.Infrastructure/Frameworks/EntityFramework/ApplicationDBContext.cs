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

        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Additional model configurations can go here
        }
    }
    
    }

