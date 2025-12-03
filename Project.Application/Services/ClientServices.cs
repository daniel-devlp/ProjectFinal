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

        public async Task<ClientDto?> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than zero.", nameof(id));
            var client = await _unitOfWork.Clients.GetByIdAsync(id);
            return client != null ? MapToDto(client) : null;
        }

        public async Task<ClientDto?> GetByIdentificationAsync(string identification)
        {
            if (string.IsNullOrWhiteSpace(identification))
                throw new ArgumentException("Identification is required.", nameof(identification));

            var client = await _unitOfWork.Clients.GetByIdentificationAsync(identification);
            return client != null ? MapToDto(client) : null;
        }

        public async Task<ClientDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required.", nameof(email));

            var client = await _unitOfWork.Clients.GetByEmailAsync(email);
            return client != null ? MapToDto(client) : null;
        }

        public async Task<PagedResult<ClientDto>> GetAllAsync(int pageNumber, int pageSize, string? searchTerm = null)
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

            // ✅ Validación mejorada de unicidad para identificación
            if (await _unitOfWork.Clients.ExistsByIdentificationAsync(clientDto.IdentificationNumber))
                throw new InvalidOperationException("Ya existe un cliente con este número de identificación.");

            // ✅ Validación de unicidad para email
            if (!string.IsNullOrWhiteSpace(clientDto.Email) &&
                await _unitOfWork.Clients.ExistsByEmailAsync(clientDto.Email))
                throw new InvalidOperationException("Ya existe un cliente con este email.");

            try
            {
                // Usar el constructor de dominio que incluye validaciones CON IdentificationType
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

            // ✅ Validar unicidad mejorada para identificación si cambia
            if (!string.Equals(existingClient.IdentificationNumber, clientDto.IdentificationNumber, StringComparison.OrdinalIgnoreCase))
            {
                if (await _unitOfWork.Clients.ExistsByIdentificationAsync(clientDto.IdentificationNumber, existingClient.ClientId))
                    throw new InvalidOperationException("Ya existe un cliente con este número de identificación.");
            }

            // ✅ Validar unicidad para email si cambia
            if (!string.Equals(existingClient.Email, clientDto.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _unitOfWork.Clients.ExistsByEmailAsync(clientDto.Email, existingClient.ClientId))
                    throw new InvalidOperationException("Ya existe un cliente con este email.");
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

                // Solo actualizar identificación si realmente cambió
                if (!string.Equals(existingClient.IdentificationNumber, clientDto.IdentificationNumber, StringComparison.OrdinalIgnoreCase) ||
                    !string.Equals(existingClient.IdentificationType, clientDto.IdentificationType, StringComparison.OrdinalIgnoreCase))
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

            // ✅ Borrado lógico en lugar de físico
            client.SoftDelete();
            _unitOfWork.Clients.Update(client);
            await _unitOfWork.SaveChangesAsync();
        }

        // ✅ Métodos nuevos para borrado lógico
        public async Task<PagedResult<ClientDto>> GetAllIncludingDeletedAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
            if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

            var (clients, totalCount) = await _unitOfWork.Clients.GetPagedIncludingDeletedAsync(pageNumber, pageSize, searchTerm);
            var clientDtos = clients.Select(MapToDto).ToList();

            return new PagedResult<ClientDto>
            {
                Items = clientDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<IEnumerable<ClientDto>> GetDeletedClientsAsync()
        {
            var clients = await _unitOfWork.Clients.GetDeletedClientsAsync();
            return clients.Select(MapToDto);
        }

        public async Task RestoreAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Id must be greater than zero.", nameof(id));

            await _unitOfWork.Clients.RestoreAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string identification)
        {
            if (string.IsNullOrWhiteSpace(identification))
                throw new ArgumentException("Identification is required.", nameof(identification));
            return await _unitOfWork.Clients.ExistsByIdentificationAsync(identification);
        }

        public async Task<int> CountAsync(string? searchTerm = null)
        {
            return await _unitOfWork.Clients.GetActiveCountAsync(); // Solo clientes activos
        }

        public async Task<int> CountAllAsync(string? searchTerm = null)
        {
            return await _unitOfWork.Clients.GetTotalCountAsync(); // Todos los clientes
        }

        private static ClientDto MapToDto(Client client)
        {
            if (client == null) return null!;
            return new ClientDto
            {
                ClientId = client.ClientId,
                IdentificationNumber = client.IdentificationNumber,
                IdentificationType = client.IdentificationType,
                FirstName = client.FirstName,
                LastName = client.LastName,
                Phone = client.Phone,
                Email = client.Email,
                Address = client.Address,
                IsActive = client.IsActive,
                CreatedAt = client.CreatedAt,
                UpdatedAt = client.UpdatedAt,
                DeletedAt = client.DeletedAt
            };
        }
    }
}
