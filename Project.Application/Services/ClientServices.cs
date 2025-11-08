using Project.Application.Dtos;
using Project.Domain.Entities;
using Project.Domain.Interfaces;

namespace Project.Application.Services
{
    public class ClientServices : IClientServices
    {
        private readonly IUnitOfWork _unitOfWork;

        public ClientServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<ClientDto> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than zero.", nameof(id));
            var client = await _unitOfWork.Clients.GetByIdAsync(id);
            return client != null ? MapToDto(client) : null;
        }

        public async Task<ClientDto> GetByIdentificationAsync(string identification)
        {
            if (string.IsNullOrWhiteSpace(identification))
                throw new ArgumentException("Identification is required.", nameof(identification));

            var client = await _unitOfWork.Clients.GetByIdentificationAsync(identification);
            return client != null ? MapToDto(client) : null;
        }

        public async Task<PagedResult<ClientDto>> GetAllAsync(int pageNumber, int pageSize, string searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            var (clients, totalCount) = await _unitOfWork.Clients.GetPagedAsync(pageNumber, pageSize, searchTerm);

            var clientDtos = clients.Select(MapToDto).ToList();

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

            // Validación de unicidad usando el nuevo método
            if (await _unitOfWork.Clients.ExistsAsync(c => c.IdentificationNumber == clientDto.IdentificationNumber))
                throw new InvalidOperationException("A client with this identification already exists.");

            try
            {
                // Usar el constructor de dominio que incluye validaciones
                var client = new Client(
                    clientDto.IdentificationType,
                    clientDto.IdentificationNumber,
                    clientDto.FirstName,
                    clientDto.LastName,
                    clientDto.Phone,
                    clientDto.Email,
                    clientDto.Address
                );

                await _unitOfWork.Clients.AddAsync(client);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Re-throw domain exceptions o convertir a application exceptions
                throw new InvalidOperationException($"Error creating client: {ex.Message}", ex);
            }
        }

        public async Task UpdateAsync(ClientUpdateDto clientDto)
        {
            if (clientDto == null) throw new ArgumentNullException(nameof(clientDto));

            var existingClient = await _unitOfWork.Clients.GetByIdAsync(clientDto.ClientId);
            if (existingClient == null)
                throw new InvalidOperationException("Client does not exist.");

            // Validar unicidad si cambia la identificación
            if (!string.Equals(existingClient.IdentificationNumber, clientDto.IdentificationNumber, StringComparison.OrdinalIgnoreCase))
            {
                if (await _unitOfWork.Clients.ExistsAsync(c => c.IdentificationNumber == clientDto.IdentificationNumber))
                    throw new InvalidOperationException("A client with this identification already exists.");
            }

            try
            {
                // Usar métodos de dominio para actualizar
                existingClient.UpdatePersonalInfo(
                    clientDto.FirstName,
                    clientDto.LastName,
                    clientDto.Phone,
                    clientDto.Email,
                    clientDto.Address
                );

                if (!string.Equals(existingClient.IdentificationNumber, clientDto.IdentificationNumber, StringComparison.OrdinalIgnoreCase))
                {
                    existingClient.UpdateIdentification(clientDto.IdentificationType, clientDto.IdentificationNumber);
                }

                _unitOfWork.Clients.Update(existingClient);
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error updating client: {ex.Message}", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than zero.", nameof(id));

            var client = await _unitOfWork.Clients.GetByIdAsync(id);
            if (client == null)
                throw new InvalidOperationException("Client does not exist.");

            _unitOfWork.Clients.Remove(client);
            await _unitOfWork.SaveChangesAsync();
        }

        public Task<bool> ExistsAsync(string identification)
        {
            if (string.IsNullOrWhiteSpace(identification))
                throw new ArgumentException("Identification is required.", nameof(identification));
            return _unitOfWork.Clients.ExistsAsync(c => c.IdentificationNumber == identification);
        }

        public Task<int> CountAsync(string searchTerm = null)
        {
            return string.IsNullOrWhiteSpace(searchTerm) 
                ? _unitOfWork.Clients.CountAsync() 
                : _unitOfWork.Clients.CountAsync(c => 
                    c.FirstName.Contains(searchTerm) || 
                    c.LastName.Contains(searchTerm) || 
                    c.IdentificationNumber.Contains(searchTerm));
        }

        private ClientDto MapToDto(Client client)
        {
            if (client == null) return null;
            return new ClientDto
            {
                ClientId = client.ClientId,
                IdentificationNumber = client.IdentificationNumber,
              //  IdentificationType = client.IdentificationType,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Phone = client.Phone,
                Email = client.Email,
                Address = client.Address
            };
        }
    }
}
