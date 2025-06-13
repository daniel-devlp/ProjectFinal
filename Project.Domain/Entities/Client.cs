using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class Client
    {
        public int ClientId { get; set; }

        public string IdentificationType { get; set; }
        public string IdentificationNumber { get; set; }


        public string FirstName { get; set; }


        public string LastName { get; set; }


        public string Phone { get; set; }


        public string Email { get; set; }


        public string Address { get; set; }

        // Navigation property
        public virtual ICollection<Invoice> Invoices { get; set; } = new HashSet<Invoice>();    }
}
