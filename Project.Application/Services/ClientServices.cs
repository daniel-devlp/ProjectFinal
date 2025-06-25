using Microsoft.EntityFrameworkCore;
using Project.Application.Dtos;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Frameworks.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.Services
{
    public class ClientServices : IClientServices
    {
        private readonly IClientRepository _clientRepository;
        private readonly ApplicationDBContext _context;
        public ClientServices(IClientRepository clientRepository, ApplicationDBContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
        }

        public async Task<ClientDto> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than zero.", nameof(id));
            var client = await _clientRepository.GetByIdAsync(id);
            return client != null ? MapToDto(client) : null;
        }

        public async Task<ClientDto> GetByIdentificationAsync(string identification)
        {
            if (string.IsNullOrWhiteSpace(identification))
                throw new ArgumentException("Identification is required.", nameof(identification));

            var client = await _clientRepository.GetByIdentificationAsync(identification);
            return client != null ? MapToDto(client) : null;
        }

        public async Task<PagedResult<ClientDto>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            var query = _context.Clients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim().ToLower();
                query = query.Where(c =>
                    (c.FirstName != null && c.FirstName.ToLower().Contains(term)) ||
                    (c.LastName != null && c.LastName.ToLower().Contains(term)) ||
                    (c.IdentificationNumber != null && c.IdentificationNumber.ToLower().Contains(term)) ||
                    (c.Email != null && c.Email.ToLower().Contains(term)) ||
                    (c.Phone != null && c.Phone.ToLower().Contains(term)) ||
                    (c.Address != null && c.Address.ToLower().Contains(term))
                );
            }

            var totalCount = await query.CountAsync();

            var clients = await query
                .OrderBy(c => c.ClientId)
                .ThenBy(c => c.LastName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            var clientDtos = clients.Select(c => new ClientDto
            {
                ClientId = c.ClientId,
                IdentificationNumber = c.IdentificationNumber,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Phone = c.Phone,
                Email = c.Email,
                Address = c.Address
            }).ToList();

            return new PagedResult<ClientDto>
            {
                Items = clientDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task AddAsync(ClientCreateDto clientDto)
        {
            if (clientDto == null) throw new ArgumentNullException(nameof(clientDto));

            // Validación de cédula (asumiendo que solo aplica para cédulas ecuatorianas)
            if (clientDto.IdentificationType == "CED" && !VerificaCedula(clientDto.IdentificationNumber))
                throw new InvalidOperationException("El número de cédula no es válido.");

            // Validación de unicidad
            if (await _clientRepository.ExistsAsync(clientDto.IdentificationNumber))
                throw new InvalidOperationException("A client with this identification already exists.");

            var client = new Client
            {
                IdentificationType = clientDto.IdentificationType,
                IdentificationNumber = clientDto.IdentificationNumber,
                FirstName = clientDto.FirstName?.Trim(),
                LastName = clientDto.LastName?.Trim(),
                Phone = clientDto.Phone?.Trim(),
                Email = clientDto.Email?.Trim(),
                Address = clientDto.Address?.Trim()
            };

            await _clientRepository.AddAsync(client);
        }

        private bool VerificaCedula(string identificationNumber)
        {
            int isNumeric;
            var total = 0;
            const int tamanoLongitudCedula = 10;
            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
            const int numeroProvincias = 24;
            const int tercerDigito = 6;

            if (int.TryParse(identificationNumber, out isNumeric) && identificationNumber.Length == tamanoLongitudCedula)
            {
                var provincia = Convert.ToInt32(string.Concat(identificationNumber[0], identificationNumber[1], string.Empty));
                var digitoTres = Convert.ToInt32(identificationNumber[2] + string.Empty);
                if ((provincia > 0 && provincia <= numeroProvincias) && digitoTres < tercerDigito)
                {
                    var digitoVerificadorRecibido = Convert.ToInt32(identificationNumber[9] + string.Empty);
                    for (var k = 0; k < coeficientes.Length; k++)
                    {
                        var valor = Convert.ToInt32(coeficientes[k] + string.Empty) * Convert.ToInt32(identificationNumber[k] + string.Empty);
                        total = valor >= 10 ? total + (valor - 9) : total + valor;
                    }
                    var digitoVerificadorObtenido = total >= 10 ? (total % 10) != 0 ? 10 - (total % 10) : (total % 10) : total;
                    return digitoVerificadorObtenido == digitoVerificadorRecibido;
                }
            }
            return false;
        }

        public async Task UpdateAsync(ClientUpdateDto clientDto)
        {
            if (clientDto == null) throw new ArgumentNullException(nameof(clientDto));

            var existingClient = await _clientRepository.GetByIdAsync(clientDto.ClientId);
            if (existingClient == null)
                throw new InvalidOperationException("Client does not exist.");

            // (Opcional) Validar unicidad si cambia la identificación
            if (!string.Equals(existingClient.IdentificationNumber, clientDto.IdentificationNumber, StringComparison.OrdinalIgnoreCase))
            {
                if (await _clientRepository.ExistsAsync(clientDto.IdentificationNumber))
                    throw new InvalidOperationException("A client with this identification already exists.");
            }

            existingClient.IdentificationType = clientDto.IdentificationType;
            existingClient.IdentificationNumber = clientDto.IdentificationNumber;
            existingClient.FirstName = clientDto.FirstName?.Trim();
            existingClient.LastName = clientDto.LastName?.Trim();
            existingClient.Phone = clientDto.Phone?.Trim();
            existingClient.Email = clientDto.Email?.Trim();
            existingClient.Address = clientDto.Address?.Trim();

            await _clientRepository.UpdateAsync(existingClient);
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than zero.", nameof(id));

            var client = await _clientRepository.GetByIdAsync(id);
            if (client == null)
                throw new InvalidOperationException("Client does not exist.");

            await _clientRepository.DeleteAsync(id);
        }

        public Task<bool> ExistsAsync(string identification)
        {
            if (string.IsNullOrWhiteSpace(identification))
                throw new ArgumentException("Identification is required.", nameof(identification));
            return _clientRepository.ExistsAsync(identification);
        }

        public Task<int> CountAsync(string searchTerm = null)
        {
            return _clientRepository.CountAsync(searchTerm?.Trim());
        }

        private ClientDto MapToDto(Client client)
        {
            if (client == null) return null;
            return new ClientDto
            {
                ClientId = client.ClientId,
                IdentificationNumber = client.IdentificationNumber,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Phone = client.Phone,
                Email = client.Email,
                Address = client.Address
            };
        }
    }
}
