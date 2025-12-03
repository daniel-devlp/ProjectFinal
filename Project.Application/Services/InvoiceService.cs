using Project.Application.Dtos;
using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Domain.Dtos;

namespace Project.Application.Services
{
    public class InvoiceService : IInvoiceServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInvoiceNumberService _invoiceNumberService;
        private readonly IClientServices _clientService;

        public InvoiceService(
            IUnitOfWork unitOfWork, 
    IInvoiceNumberService invoiceNumberService,
      IClientServices clientService)
        {
      _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    _invoiceNumberService = invoiceNumberService ?? throw new ArgumentNullException(nameof(invoiceNumberService));
_clientService = clientService ?? throw new ArgumentNullException(nameof(clientService));
        }

        public async Task<InvoiceDto?> GetByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(id));
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
        return invoice != null ? ToInvoiceDto(invoice) : null;
        }

        public async Task<PagedResult<InvoiceDto>> GetAllAsync(int pageNumber, int pageSize, string? searchTerm = null)
        {
            if (pageNumber <= 0) throw new ArgumentException("Page number must be greater than zero.", nameof(pageNumber));
 if (pageSize <= 0) throw new ArgumentException("Page size must be greater than zero.", nameof(pageSize));

     var (invoices, totalCount) = await _unitOfWork.Invoices.GetPagedAsync(pageNumber, pageSize, searchTerm);
        var invoiceDtos = invoices.Select(ToInvoiceDto).ToList();

            return new PagedResult<InvoiceDto>
            {
Items = invoiceDtos,
             TotalCount = totalCount,
           PageNumber = pageNumber,
      PageSize = pageSize
         };
        }

        public async Task<InvoiceDto> AddAsync(InvoiceCreateDto invoiceDto, string currentUserId)
        {
    if (invoiceDto == null) throw new ArgumentNullException(nameof(invoiceDto));
          if (string.IsNullOrWhiteSpace(currentUserId)) throw new ArgumentException("User ID is required.", nameof(currentUserId));

      await _unitOfWork.BeginTransactionAsync();
      try
            {
       // ✅ Validaciones de integridad referencial
          if (!await _unitOfWork.Invoices.ClientExistsAsync(invoiceDto.ClientId))
   throw new InvalidOperationException("El cliente especificado no existe.");

                if (!await _unitOfWork.Invoices.UserExistsAsync(currentUserId))
         throw new InvalidOperationException("El usuario actual no existe.");

    // ✅ Validar detalles y stock
                foreach (var detail in invoiceDto.InvoiceDetails)
            {
         if (!await _unitOfWork.Invoices.ProductExistsAsync(detail.ProductId))
    throw new InvalidOperationException($"El producto con ID {detail.ProductId} no existe.");

           if (!await _unitOfWork.Products.HasStockAsync(detail.ProductId, detail.Quantity))
          throw new InvalidOperationException($"Stock insuficiente para el producto con ID {detail.ProductId}.");
                }

  // ✅ Generar número de factura automáticamente
                var invoiceNumber = await _invoiceNumberService.GenerateInvoiceNumberAsync();

       // ✅ Crear factura usando constructor de dominio
                var invoice = new Invoice(invoiceDto.ClientId, currentUserId, invoiceDto.Observations ?? "");
                invoice.SetInvoiceNumber(invoiceNumber);

   // ✅ Obtener productos y agregar detalles con precios actuales
        foreach (var detailDto in invoiceDto.InvoiceDetails)
    {
    var product = await _unitOfWork.Products.GetByIdAsync(detailDto.ProductId);
         if (product != null)
         {
invoice.AddDetail(detailDto.ProductId, detailDto.Quantity, product.Price);
       }
       }

     invoice.Finalize(); // Cambiar estado a finalizada

    await _unitOfWork.Invoices.AddAsync(invoice);
          await _unitOfWork.SaveChangesAsync();

    // ✅ Decrementar stock después de guardar la factura
     foreach (var detail in invoice.InvoiceDetails)
      {
             await _unitOfWork.Products.DecreaseStockAsync(detail.ProductID, detail.Quantity);
      }

     await _unitOfWork.SaveChangesAsync();
  await _unitOfWork.CommitTransactionAsync();

      return ToInvoiceDto(invoice);
     }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
   throw;
            }
        }

     /// <summary>
      /// ✅ Crea una factura asociada automáticamente al usuario autenticado
 /// Obtiene o crea un cliente basado en los datos del usuario
        /// </summary>
  public async Task<InvoiceDto> CreateInvoiceForUserAsync(string userId, InvoiceCreateForUserDto invoiceDto)
        {
       if (string.IsNullOrWhiteSpace(userId)) 
                throw new ArgumentException("User ID is required.", nameof(userId));
    if (invoiceDto == null) 
    throw new ArgumentNullException(nameof(invoiceDto));

            await _unitOfWork.BeginTransactionAsync();
  try
         {
    // ✅ 1. Validar que el usuario existe
     if (!await _unitOfWork.Invoices.UserExistsAsync(userId))
          throw new InvalidOperationException("El usuario actual no existe.");

     // ✅ 2. Obtener o crear cliente asociado al usuario
     var clientId = await GetOrCreateClientForUserAsync(userId);

          // ✅ 3. Validar detalles y stock
       foreach (var detail in invoiceDto.InvoiceDetails)
         {
         if (!await _unitOfWork.Invoices.ProductExistsAsync(detail.ProductId))
   throw new InvalidOperationException($"El producto con ID {detail.ProductId} no existe.");

   if (!await _unitOfWork.Products.HasStockAsync(detail.ProductId, detail.Quantity))
throw new InvalidOperationException($"Stock insuficiente para el producto con ID {detail.ProductId}.");
  }

  // ✅ 4. Generar número de factura automáticamente
  var invoiceNumber = await _invoiceNumberService.GenerateInvoiceNumberAsync();

           // ✅ 5. Crear factura usando constructor de dominio
    var invoice = new Invoice(clientId, userId, invoiceDto.Observations ?? "");
    invoice.SetInvoiceNumber(invoiceNumber);

       // ✅ 6. Agregar detalles con precios actuales
 foreach (var detailDto in invoiceDto.InvoiceDetails)
  {
var product = await _unitOfWork.Products.GetByIdAsync(detailDto.ProductId);
        if (product != null)
       {
    invoice.AddDetail(detailDto.ProductId, detailDto.Quantity, product.Price);
          }
    }

     invoice.Finalize(); // Cambiar estado a finalizada automáticamente

      await _unitOfWork.Invoices.AddAsync(invoice);
     await _unitOfWork.SaveChangesAsync();

     // ✅ 7. Decrementar stock después de guardar la factura
             foreach (var detail in invoice.InvoiceDetails)
        {
        await _unitOfWork.Products.DecreaseStockAsync(detail.ProductID, detail.Quantity);
           }

     await _unitOfWork.SaveChangesAsync();
     await _unitOfWork.CommitTransactionAsync();

 return ToInvoiceDto(invoice);
            }
         catch
   {
  await _unitOfWork.RollbackTransactionAsync();
       throw;
  }
        }

        /// <summary>
   /// ✅ Obtiene o crea un cliente asociado al usuario especificado
      /// GARANTIZA que el cliente tenga los datos del usuario autenticado
        /// </summary>
        private async Task<int> GetOrCreateClientForUserAsync(string userId)
        {
            try
 {
          // ✅ 1. Obtener datos del usuario desde el repositorio
   var userData = await _unitOfWork.Invoices.GetUserDataAsync(userId);
            if (userData == null)
       throw new InvalidOperationException("No se pudieron obtener los datos del usuario.");

    System.Diagnostics.Debug.WriteLine($"🔍 Datos del usuario obtenidos: UserId={userData.UserId}, UserName={userData.UserName}, Email={userData.Email}, Identification={userData.Identification}");

   // ✅ 2. PRIMERA BÚSQUEDA: Por identificación del usuario (si tiene)
           if (!string.IsNullOrWhiteSpace(userData.Identification) && userData.Identification != "9999999999")
                {
   var clientByIdentification = await _clientService.GetByIdentificationAsync(userData.Identification);
       if (clientByIdentification != null)
 {
     System.Diagnostics.Debug.WriteLine($"✅ Cliente encontrado por identificación: ClientId={clientByIdentification.ClientId}");
  return clientByIdentification.ClientId;
    }
         }

      // ✅ 3. SEGUNDA BÚSQUEDA: Por email del usuario (más seguro para asociar)
              if (!string.IsNullOrWhiteSpace(userData.Email))
                {
         System.Diagnostics.Debug.WriteLine($"🔍 Buscando cliente por email: {userData.Email}");
            
        // Aquí necesitamos buscar por email - vamos a agregar este método
            var clientByEmail = await TryFindClientByEmailAsync(userData.Email);
 if (clientByEmail != null)
         {
                   System.Diagnostics.Debug.WriteLine($"✅ Cliente encontrado por email: ClientId={clientByEmail.ClientId}");
   return clientByEmail.ClientId;
        }
            }

         // ✅ 4. NO EXISTE CLIENTE - CREAR UNO NUEVO CON DATOS DEL USUARIO
        System.Diagnostics.Debug.WriteLine($"📝 No existe cliente para el usuario, creando uno nuevo...");

             var createClientDto = new ClientCreateDto
         {
                 IdentificationNumber = !string.IsNullOrWhiteSpace(userData.Identification) 
 ? userData.Identification 
          : GenerateUniqueTemporaryId(userData.UserId),
             IdentificationType = "Cedula", // Por defecto
             FirstName = ExtractFirstName(userData.UserName ?? "Usuario"),
   LastName = ExtractLastName(userData.UserName ?? "Usuario") ?? "Sistema",
     Phone = userData.PhoneNumber ?? "",
     Email = userData.Email ?? $"user-{userData.UserId}@temp.com", // Email temporal si no tiene
        Address = "Dirección no especificada"
  };

      System.Diagnostics.Debug.WriteLine($"✅ Creando cliente: {createClientDto.FirstName} {createClientDto.LastName}, Email={createClientDto.Email}, ID={createClientDto.IdentificationNumber}");

     // ✅ 5. CREAR EL CLIENTE
   await _clientService.AddAsync(createClientDto);

  // ✅ 6. OBTENER EL CLIENTE RECIÉN CREADO
   var newClient = await _clientService.GetByIdentificationAsync(createClientDto.IdentificationNumber);
         if (newClient == null)
                {
          // Fallback: si no lo encuentra por identificación, buscar por email
       newClient = await TryFindClientByEmailAsync(createClientDto.Email);
                }

       if (newClient == null)
        throw new InvalidOperationException("Error crítico: No se pudo crear o recuperar el cliente para el usuario.");

      System.Diagnostics.Debug.WriteLine($"✅ Cliente creado exitosamente: ClientId={newClient.ClientId}, Nombre={newClient.FirstName} {newClient.LastName}");
                return newClient.ClientId;
    }
            catch (Exception ex)
            {
     System.Diagnostics.Debug.WriteLine($"❌ Error en GetOrCreateClientForUserAsync: {ex.Message}");
       System.Diagnostics.Debug.WriteLine($"📋 Stack trace: {ex.StackTrace}");
    throw new InvalidOperationException($"Error al obtener o crear cliente para el usuario: {ex.Message}", ex);
     }
        }

        /// <summary>
        /// Busca un cliente por email usando el servicio de clientes
        /// </summary>
  private async Task<ClientDto?> TryFindClientByEmailAsync(string email)
 {
            try
    {
     return await _clientService.GetByEmailAsync(email);
   }
      catch (Exception ex)
          {
      System.Diagnostics.Debug.WriteLine($"⚠️ Error buscando cliente por email: {ex.Message}");
      return null;
      }
  }

        /// <summary>
        /// Genera un ID temporal único garantizado para casos donde el usuario no tiene identificación
     /// </summary>
        private static string GenerateUniqueTemporaryId(string userId)
        {
 // Crear un ID más único basado en timestamp + hash del userId
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
  var hash = userId.GetHashCode();
      var combined = Math.Abs(timestamp + hash);
  
       // Tomar los últimos 10 dígitos y asegurar que no sea 9999999999
 var tempId = (combined % 1000000000).ToString("D10");
      
            // Si resulta en el ID temporal por defecto, modificarlo
  if (tempId == "9999999999")
    tempId = "9999999998";
         
      return tempId;
        }

        /// <summary>
        /// Extrae el primer nombre del nombre completo del usuario
        /// </summary>
     private static string ExtractFirstName(string fullName)
      {
            if (string.IsNullOrWhiteSpace(fullName))
   return "Usuario";

            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
          return parts.Length > 0 ? parts[0] : "Usuario";
        }

        /// <summary>
   /// Extrae el apellido del nombre completo del usuario
      /// </summary>
        private static string? ExtractLastName(string fullName)
        {
  if (string.IsNullOrWhiteSpace(fullName))
                return "Sistema";

            var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    return parts.Length > 1 ? string.Join(" ", parts[1..]) : "Sistema";
        }

        public async Task<InvoiceDto> UpdateAsync(InvoiceUpdateDto invoiceDto)
        {
  if (invoiceDto == null) throw new ArgumentNullException(nameof(invoiceDto));

            await _unitOfWork.BeginTransactionAsync();
            try
            {
       var existingInvoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceDto.InvoiceId);
   if (existingInvoice == null)
        throw new InvalidOperationException("La factura no existe.");

          if (!existingInvoice.CanBeModified())
     throw new InvalidOperationException("No se puede modificar una factura finalizada o cancelada.");

 // ✅ Validaciones de integridad referencial
         if (!await _unitOfWork.Invoices.ClientExistsAsync(invoiceDto.ClientId))
     throw new InvalidOperationException("El cliente especificado no existe.");

 // ✅ Restaurar stock de productos existentes
                foreach (var existingDetail in existingInvoice.InvoiceDetails.ToList())
     {
         await _unitOfWork.Products.IncreaseStockAsync(existingDetail.ProductID, existingDetail.Quantity);
            existingInvoice.RemoveDetail(existingDetail.ProductID);
        }

          // ✅ Validar nuevos productos y stock
      foreach (var detail in invoiceDto.InvoiceDetails)
        {
      if (!await _unitOfWork.Invoices.ProductExistsAsync(detail.ProductId))
     throw new InvalidOperationException($"El producto con ID {detail.ProductId} no existe.");

         if (!await _unitOfWork.Products.HasStockAsync(detail.ProductId, detail.Quantity))
            throw new InvalidOperationException($"Stock insuficiente para el producto con ID {detail.ProductId}.");
       }

            // ✅ Actualizar campos básicos
          existingInvoice.SetClient(invoiceDto.ClientId);
 existingInvoice.Observations = invoiceDto.Observations?.Trim() ?? "";

            // ✅ Agregar nuevos detalles con precios actuales
       foreach (var detailDto in invoiceDto.InvoiceDetails)
      {
           var product = await _unitOfWork.Products.GetByIdAsync(detailDto.ProductId);
             if (product != null)
   {
     existingInvoice.AddDetail(detailDto.ProductId, detailDto.Quantity, product.Price);
           }
          }

      _unitOfWork.Invoices.Update(existingInvoice);
                await _unitOfWork.SaveChangesAsync();

         // ✅ Decrementar stock de nuevos productos
   foreach (var detail in existingInvoice.InvoiceDetails)
            {
   await _unitOfWork.Products.DecreaseStockAsync(detail.ProductID, detail.Quantity);
           }

      await _unitOfWork.SaveChangesAsync();
    await _unitOfWork.CommitTransactionAsync();

                return ToInvoiceDto(existingInvoice);
     }
      catch
         {
          await _unitOfWork.RollbackTransactionAsync();
    throw;
        }
     }

        public async Task DeleteAsync(int id)
        {
   if (id <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(id));
      
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
    if (invoice == null)
         throw new InvalidOperationException("La factura no existe.");

    await _unitOfWork.BeginTransactionAsync();
            try
            {
        // ✅ Restaurar stock antes de eliminación lógica
                foreach (var detail in invoice.InvoiceDetails)
           {
  await _unitOfWork.Products.IncreaseStockAsync(detail.ProductID, detail.Quantity);
       }

         // ✅ Borrado lógico
           invoice.SoftDelete("Eliminada por usuario");
  _unitOfWork.Invoices.Update(invoice);
           await _unitOfWork.SaveChangesAsync();
          await _unitOfWork.CommitTransactionAsync();
       }
            catch
  {
       await _unitOfWork.RollbackTransactionAsync();
                throw;
 }
        }

        // ✅ Métodos nuevos para borrado lógico
      public async Task<PagedResult<InvoiceDto>> GetAllIncludingDeletedAsync(int pageNumber, int pageSize, string? searchTerm = null)
   {
          var (invoices, totalCount) = await _unitOfWork.Invoices.GetPagedIncludingDeletedAsync(pageNumber, pageSize, searchTerm);
            var invoiceDtos = invoices.Select(ToInvoiceDto).ToList();

            return new PagedResult<InvoiceDto>
            {
   Items = invoiceDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
  PageSize = pageSize
        };
      }

        public async Task<IEnumerable<InvoiceDto>> GetDeletedInvoicesAsync()
        {
            var invoices = await _unitOfWork.Invoices.GetDeletedInvoicesAsync();
            return invoices.Select(ToInvoiceDto);
        }

        public async Task RestoreAsync(int id)
        {
  if (id <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(id));

 await _unitOfWork.Invoices.RestoreAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

    public async Task<InvoiceDto> FinalizeAsync(InvoiceFinalizeDto dto)
        {
   var invoice = await _unitOfWork.Invoices.GetByIdAsync(dto.InvoiceId);
         if (invoice == null)
          throw new InvalidOperationException("La factura no existe.");

        if (invoice.Status != InvoiceStatus.Draft)
         throw new InvalidOperationException("Solo se pueden finalizar facturas en borrador.");

   if (!invoice.InvoiceDetails.Any())
      throw new InvalidOperationException("No se puede finalizar una factura sin detalles.");

            invoice.Finalize();
      _unitOfWork.Invoices.Update(invoice);
   await _unitOfWork.SaveChangesAsync();

            return ToInvoiceDto(invoice);
        }

        public async Task<InvoiceDto> CancelAsync(InvoiceCancelDto dto)
        {
            await _unitOfWork.BeginTransactionAsync();
   try
            {
    var invoice = await _unitOfWork.Invoices.GetByIdAsync(dto.InvoiceId);
        if (invoice == null)
                throw new InvalidOperationException("La factura no existe.");

           if (invoice.Status == InvoiceStatus.Cancelled)
        throw new InvalidOperationException("La factura ya está cancelada.");

      // ✅ Restaurar stock si se cancela una factura finalizada
   if (invoice.Status == InvoiceStatus.Finalized)
         {
    foreach (var detail in invoice.InvoiceDetails)
         {
          await _unitOfWork.Products.IncreaseStockAsync(detail.ProductID, detail.Quantity);
              }
                }

         invoice.Cancel(dto.Reason);
            _unitOfWork.Invoices.Update(invoice);
   await _unitOfWork.SaveChangesAsync();
      await _unitOfWork.CommitTransactionAsync();

     return ToInvoiceDto(invoice);
     }
            catch
            {
           await _unitOfWork.RollbackTransactionAsync();
          throw;
   }
        }

        public async Task<IEnumerable<InvoiceDto>> SearchAsync(InvoiceSearchDto searchDto)
        {
   var invoices = await _unitOfWork.Invoices.SearchAsync(
      searchDto.FromDate,
             searchDto.ToDate,
      searchDto.ClientId,
        Enum.TryParse<InvoiceStatus>(searchDto.Status, out var status) ? status : null,
      searchDto.SearchTerm);

            return invoices.Select(ToInvoiceDto);
        }

    public async Task<int> CountAsync(string? searchTerm = null)
        {
     return await _unitOfWork.Invoices.GetActiveCountAsync();
      }

        public async Task<int> CountAllAsync(string? searchTerm = null)
        {
        return await _unitOfWork.Invoices.GetTotalCountAsync();
        }

        public async Task<IEnumerable<InvoiceDetailDto>> GetInvoiceDetailsAsync(int invoiceId)
        {
            if (invoiceId <= 0) throw new ArgumentException("Invoice ID must be greater than zero.", nameof(invoiceId));
   var details = await _unitOfWork.Invoices.GetInvoiceDetailsAsync(invoiceId);
            return details?.Select(ToInvoiceDetailDto) ?? Enumerable.Empty<InvoiceDetailDto>();
     }

        // ✅ Método de mapeo mejorado
        private static InvoiceDto ToInvoiceDto(Invoice invoice)
{
   if (invoice == null) return null!;

         return new InvoiceDto
 {
                InvoiceId = invoice.InvoiceId,
          InvoiceNumber = invoice.InvoiceNumber,
     ClientId = invoice.ClientId,
   UserId = invoice.UserId,
              IssueDate = invoice.IssueDate,
          Subtotal = invoice.Subtotal,
             Tax = invoice.Tax,
       Total = invoice.Total,
              Observations = invoice.Observations,
      IsActive = invoice.IsActive,
       Status = invoice.Status.ToString(),
        CancelReason = invoice.CancelReason,
             CreatedAt = invoice.CreatedAt,
        UpdatedAt = invoice.UpdatedAt,
       DeletedAt = invoice.DeletedAt,
           Client = invoice.Client != null ? new ClientDto
  {
     ClientId = invoice.Client.ClientId,
           IdentificationNumber = invoice.Client.IdentificationNumber,
  IdentificationType = invoice.Client.IdentificationType,
           FirstName = invoice.Client.FirstName,
        LastName = invoice.Client.LastName,
         Phone = invoice.Client.Phone,
     Email = invoice.Client.Email,
                Address = invoice.Client.Address,
       IsActive = invoice.Client.IsActive,
    CreatedAt = invoice.Client.CreatedAt,
UpdatedAt = invoice.Client.UpdatedAt,
         DeletedAt = invoice.Client.DeletedAt
        } : null,
     InvoiceDetails = invoice.InvoiceDetails?.Select(ToInvoiceDetailDto).ToList() ?? new List<InvoiceDetailDto>()
 };
        }

      private static InvoiceDetailDto ToInvoiceDetailDto(InvoiceDetail detail)
        {
          if (detail == null) return null!;

  return new InvoiceDetailDto
            {
           InvoiceDetailId = detail.InvoiceDetailId,
       InvoiceId = detail.InvoiceID,
       ProductId = detail.ProductID,
      Quantity = detail.Quantity,
         UnitPrice = detail.UnitPrice,
     Subtotal = detail.Subtotal,
     Product = detail.Product != null ? new ProductDto
            {
       ProductId = detail.Product.ProductId,
    Code = detail.Product.Code,
   Name = detail.Product.Name,
              Description = detail.Product.Description,
        Price = detail.Product.Price,
          Stock = detail.Product.Stock,
    IsActive = detail.Product.IsActive,
  ImageUri = detail.Product.ImageUri,
           CreatedAt = detail.Product.CreatedAt,
         UpdatedAt = detail.Product.UpdatedAt,
     DeletedAt = detail.Product.DeletedAt
        } : null
    };
        }
    }
}
