using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class Product
    {
        public int ProductId { get; set; }


        public string Code { get; set; }


        public string Name { get; set; }


        public string Description { get; set; }


        public decimal Price { get; set; }


        public int Stock { get; set; } = 0;

        public bool IsActive { get; set; } = true;


        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new HashSet<InvoiceDetail>();
    }
}
