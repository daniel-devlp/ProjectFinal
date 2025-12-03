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
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public string ImageUri { get; set; } = string.Empty;
        
        // ✅ Campos adicionales para auditoría y borrado lógico
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; } // ✅ Campo para auditoría de eliminación
        
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new HashSet<InvoiceDetail>();
        public virtual ICollection<ShoppingCart> ShoppingCartItems { get; set; } = new HashSet<ShoppingCart>();

        // ✅ Constructor con validaciones básicas
        public Product()
        {
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        public Product(string code, string name, string description, decimal price, int stock, string imageUri = "")
        {
            SetCode(code);
            SetName(name);
            SetDescription(description);
            SetPrice(price);
            SetStock(stock);
            ImageUri = imageUri ?? string.Empty;
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }

        // ✅ Métodos para validaciones de dominio
        public void SetCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Product code is required");
            
            if (code.Length < 3 || code.Length > 20)
                throw new ArgumentException("Product code must be between 3 and 20 characters");

            Code = code.Trim().ToUpperInvariant();
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Product name is required");
                
            if (name.Length > 200)
                throw new ArgumentException("Product name cannot exceed 200 characters");

            Name = name.Trim();
        }

        public void SetDescription(string description)
        {
            Description = description?.Trim() ?? string.Empty;
        }

        public void SetPrice(decimal price)
        {
            if (price <= 0)
                throw new ArgumentException("Product price must be greater than zero");
            
            Price = price;
        }

        public void SetStock(int stock)
        {
            if (stock < 0)
                throw new ArgumentException("Product stock cannot be negative");
        
            Stock = stock;
        }

        public void UpdateProduct(string name, string description, decimal price, int stock, string? imageUri = null)
        {
            SetName(name);
            SetDescription(description);
            SetPrice(price);
            SetStock(stock);
            
            if (imageUri != null)
                ImageUri = imageUri;
        
            UpdatedAt = DateTime.UtcNow;
        }

        public void DecreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");
            
            if (Stock < quantity)
                throw new InvalidOperationException("Insufficient stock");
        
            Stock -= quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        public void IncreaseStock(int quantity)
        {
            if (quantity <= 0)
                throw new ArgumentException("Quantity must be positive");
        
            Stock += quantity;
            UpdatedAt = DateTime.UtcNow;
        }

        // ✅ Método para borrado lógico
        public void SoftDelete()
        {
            IsActive = false;
            DeletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // ✅ Método para restaurar producto eliminado
        public void Restore()
        {
            IsActive = true;
            DeletedAt = null;
            UpdatedAt = DateTime.UtcNow;
        }

        // ✅ Método para verificar disponibilidad
        public bool HasStock(int requiredQuantity = 1)
        {
            return IsActive && Stock >= requiredQuantity;
        }
    }
}
