using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Project.Infrastructure.Frameworks.EntityFramework
{
    public class DesingDBContextFactory : IDesignTimeDbContextFactory<ApplicationDBContext>
    {
        public ApplicationDBContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDBContext>();

            // Aquí va tu cadena de conexión
            var connectionString = "Data Source=DANNY\\MULTIDIMENSIONAL;Initial Catalog=InvoiceDB;Integrated Security=True;Encrypt=False;Trust Server Certificate=True";
            optionsBuilder.UseSqlServer(connectionString);

            return new ApplicationDBContext(optionsBuilder.Options);
        }
    }
}
