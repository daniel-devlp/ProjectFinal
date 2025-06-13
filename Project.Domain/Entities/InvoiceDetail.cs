using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class InvoiceDetail
    {
        public int InvoiceDetailId { get; set; }
        
        public int InvoiceID { get; set; }


        public int ProductID { get; set; }


        public int Quantity { get; set; }


        public decimal UnitPrice { get; set; }


        public decimal Subtotal { get; set; }
        // Propiedades de navegación
        public virtual Invoice Invoice { get; set; }
        public virtual Product Product { get; set; }
    }
}
