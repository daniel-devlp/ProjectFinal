using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Project.Domain.ValueObjects;
using Project.Domain.Exceptions;

namespace Project.Domain.Entities
{
    public class Client
    {
        public int ClientId { get; set; }
        
        // Campos originales para compatibilidad con BD existente
        public string IdentificationType { get; set; } = string.Empty;
        public string IdentificationNumber { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        
        // Campos adicionales para Clean Architecture
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Invoice> Invoices { get; set; } = new HashSet<Invoice>();

        // Constructor sin parámetros para EF
        public Client() 
        { 
            CreatedAt = DateTime.UtcNow;
        }

        // Constructor con validaciones de dominio
        public Client(string identificationType, string identificationNumber, 
                     string firstName, string lastName, string phone, 
                     string email, string address)
        {
            SetIdentification(identificationType, identificationNumber);
            SetPersonalInfo(firstName, lastName, phone, email, address);
            CreatedAt = DateTime.UtcNow;
        }

        public void UpdatePersonalInfo(string firstName, string lastName, 
                                     string phone, string email, string address)
        {
            SetPersonalInfo(firstName, lastName, phone, email, address);
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateIdentification(string identificationType, string identificationNumber)
        {
            SetIdentification(identificationType, identificationNumber);
            UpdatedAt = DateTime.UtcNow;
        }

        private void SetIdentification(string type, string number)
        {
            try
            {
                // Validar usando Value Object pero mantener en propiedades primitivas
                var identification = new Identification(type, number);
                IdentificationType = identification.Type;
                IdentificationNumber = identification.Number;
            }
            catch (ArgumentException ex)
            {
                throw new ClientDomainException($"Invalid identification: {ex.Message}");
            }
        }

        private void SetPersonalInfo(string firstName, string lastName, 
                                   string phone, string email, string address)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ClientDomainException("First name is required");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new ClientDomainException("Last name is required");

            if (!string.IsNullOrWhiteSpace(email) && !IsValidEmail(email))
                throw new ClientDomainException("Invalid email format");

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Phone = phone?.Trim() ?? string.Empty;
            Email = email?.Trim() ?? string.Empty;
            Address = address?.Trim() ?? string.Empty;
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public string GetFullName() => $"{FirstName} {LastName}";

        // Método helper para obtener el Value Object cuando sea necesario
        public Identification GetIdentification() => new Identification(IdentificationType, IdentificationNumber);
    }
}
