using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
       

        public string InvoiceNumber { get; set; }

        public int ClientId { get; set; }
        public string UserId { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;


        public decimal Subtotal { get; set; }


        public decimal Tax { get; set; } = 0;


        public decimal Total { get; set; }


        public string Observations { get; set; }

        // Navigation properties

        public virtual Client Client { get; set; }
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new HashSet<InvoiceDetail>();
    }
}
