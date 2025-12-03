# Funcionalidad: Crear Facturas Automáticas para Usuario Autenticado

Esta funcionalidad permite que al registrar una factura desde el carrito de compras o aplicación móvil, el cliente sea automáticamente el usuario que está autenticado.

## ?? Funcionalidades Implementadas

### 1. **Nuevo Endpoint para Facturas de Usuario**
```http
POST /api/invoice/create-for-current-user
Authorization: Bearer {token}
Content-Type: application/json

{
  "observations": "Compra desde carrito móvil",
  "invoiceDetails": [
    {
  "productId": 1,
      "quantity": 2
    },
    {
      "productId": 3,
    "quantity": 1
 }
  ]
}
```

### 2. **Flujo Automático**
1. **Usuario autenticado** realiza una compra desde carrito o móvil
2. **Sistema obtiene automáticamente** el `userId` del token JWT
3. **Se busca o crea** un cliente asociado a ese usuario:
   - Si el usuario ya tiene un cliente con su misma identificación ? Se usa ese cliente
   - Si no existe ? Se crea automáticamente un cliente con los datos del usuario
4. **Se crea la factura** con el `ClientId` del cliente asociado
5. **La factura queda finalizada** automáticamente y se descuenta el stock

### 3. **Ventajas de la Implementación**

? **Automático:** No requiere que el frontend envíe `ClientId`  
? **Seguro:** Usa el token JWT para identificar al usuario  
? **Flexible:** Crea cliente automáticamente si no existe  
? **Consistente:** Mantiene la relación usuario-cliente-factura  
? **Retrocompatible:** El endpoint original sigue funcionando para administradores  

### 4. **DTOs Utilizados**

#### **InvoiceCreateForUserDto** (Nuevo)
```csharp
public class InvoiceCreateForUserDto
{
    public string? Observations { get; set; }
    public List<InvoiceDetailCreateDto> InvoiceDetails { get; set; }
}
```

#### **Respuesta Exitosa**
```json
{
  "success": true,
  "message": "Factura creada exitosamente para el usuario actual",
  "invoice": {
    "invoiceId": 123,
  "invoiceNumber": "FAC-2024-000123",
    "clientId": 456,
    "userId": "user-guid-here",
    "total": 35.98,
  "status": "Finalized"
  }
}
```

## ?? Implementación Técnica

### **Arquitectura Limpia Mantenida**
- **Controlador:** Maneja autenticación y validaciones
- **Servicio:** Contiene lógica de negocio para obtener/crear cliente
- **Repositorio:** Accede a datos del usuario sin violar capas
- **Dominio:** Mantiene reglas de negocio intactas

### **Gestión de Clientes Automática**
```csharp
private async Task<int> GetOrCreateClientForUserAsync(string userId)
{
    // 1. Obtener datos del usuario desde el repositorio
    var userData = await _unitOfWork.Invoices.GetUserDataAsync(userId);
    
    // 2. Buscar cliente existente por identificación
    if (!string.IsNullOrWhiteSpace(userData.Identification))
    {
        var existingClient = await _clientService.GetByIdentificationAsync(userData.Identification);
        if (existingClient != null) return existingClient.ClientId;
    }
    
    // 3. Crear cliente nuevo basado en datos del usuario
    var createClientDto = new ClientCreateDto
  {
        IdentificationNumber = userData.Identification ?? "9999999999",
        IdentificationType = "Cedula",
        FirstName = userData.UserName ?? "Usuario",
  LastName = "Sistema",
        Phone = userData.PhoneNumber ?? "",
        Email = userData.Email ?? "",
        Address = "Dirección no especificada"
    };
    
    await _clientService.AddAsync(createClientDto);
    var newClient = await _clientService.GetByIdentificationAsync(createClientDto.IdentificationNumber);
    return newClient.ClientId;
}
```

## ?? Uso desde Frontend/Móvil

### **Ejemplo para React/Flutter**
```javascript
// Al finalizar compra en carrito
const createInvoiceForUser = async (cartItems) => {
    const invoiceData = {
     observations: "Compra desde carrito móvil",
   invoiceDetails: cartItems.map(item => ({
  productId: item.productId,
            quantity: item.quantity
        }))
    };
    
    const response = await fetch('/api/invoice/create-for-current-user', {
        method: 'POST',
  headers: {
     'Authorization': `Bearer ${userToken}`,
          'Content-Type': 'application/json'
    },
      body: JSON.stringify(invoiceData)
    });
    
    if (response.ok) {
        const result = await response.json();
        console.log('Factura creada:', result.invoice);
        // Limpiar carrito, mostrar confirmación, etc.
    }
};
```

## ??? Validaciones y Seguridad

- ? **Autenticación requerida:** Solo usuarios autenticados pueden usar el endpoint
- ? **Validación de stock:** Se verifica disponibilidad antes de crear factura
- ? **Transacciones:** Todo o nada, rollback automático en caso de error
- ? **Gestión de errores:** Mensajes descriptivos para cada caso de error
- ? **Auditoría:** Se mantiene registro de quién creó cada factura

Esta implementación mantiene la arquitectura limpia y permite una experiencia fluida para usuarios finales en carrito de compras y aplicaciones móviles.