# API Documentation - Parte 4: Gestión de Facturas

## ?? Índice
- [Operaciones CRUD de Facturas](#operaciones-crud-de-facturas)
- [Operaciones Especiales](#operaciones-especiales)
- [Búsqueda Avanzada](#búsqueda-avanzada)
- [Gestión de Borrado Lógico](#gestión-de-borrado-lógico)
- [Detalles de Factura](#detalles-de-factura)
- [Estadísticas](#estadísticas)

---

## ?? Operaciones CRUD de Facturas

### GET `/api/Invoice/{id}`
Obtiene una factura específica por ID (solo facturas activas).

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer): ID de la factura

**Respuestas:**

**? 200 OK**
```json
{
  "invoiceId": 1,
  "invoiceNumber": "FAC-2024-00001",
  "clientId": 5,
  "userId": "user-guid-1234",
  "issueDate": "2024-06-15T10:30:00Z",
  "subtotal": 1500.00,
  "tax": 180.00,
  "total": 1680.00,
  "observations": "Cliente preferencial - descuento aplicado",
  "isActive": true,
  "status": "Finalizada",
  "cancelReason": null,
  "createdAt": "2024-06-15T10:30:00Z",
  "updatedAt": null,
  "deletedAt": null,
  "client": {
    "clientId": 5,
    "firstName": "Juan",
    "lastName": "Pérez",
    "email": "juan@example.com"
  },
  "invoiceDetails": [
    {
      "invoiceDetailId": 1,
      "productId": 10,
      "quantity": 2,
      "unitPrice": 750.00,
 "subtotal": 1500.00,
      "productName": "Laptop Dell XPS 15"
    }
  ]
}
```

**? 404 Not Found**
```json
{
  "message": "Invoice not found"
}
```

---

### GET `/api/Invoice`
Obtiene todas las facturas activas con paginación y búsqueda.

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
```

**Query Parameters:**
- `pageNumber` (integer, opcional): Número de página (default: 1)
- `pageSize` (integer, opcional): Tamaño de página (default: 10)
- `searchTerm` (string, opcional): Término de búsqueda (busca en número de factura, cliente)

**Ejemplo:**
```
GET /api/Invoice?pageNumber=1&pageSize=20&searchTerm=Juan
```

**Respuestas:**

**? 200 OK**
```json
{
  "items": [
    {
      "invoiceId": 1,
      "invoiceNumber": "FAC-2024-00001",
      "clientId": 5,
      "userId": "user-guid-1234",
      "issueDate": "2024-06-15T10:30:00Z",
      "subtotal": 1500.00,
      "tax": 180.00,
   "total": 1680.00,
      "status": "Finalizada",
 "client": {
   "firstName": "Juan",
        "lastName": "Pérez"
    }
    }
  ],
  "totalCount": 250,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 25,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

### POST `/api/Invoice`
Crea una nueva factura con número automático y validaciones completas.

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "clientId": 5,
  "observations": "Cliente preferencial - descuento del 10%",
  "invoiceDetails": [
    {
"productId": 10,
      "quantity": 2,
      "unitPrice": 750.00
    },
    {
      "productId": 15,
      "quantity": 1,
      "unitPrice": 299.99
    }
  ]
}
```

**Validaciones del Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `clientId` | integer | ? | Debe ser mayor a 0, cliente debe existir y estar activo |
| `observations` | string | ? | Máx 500 caracteres |
| `invoiceDetails` | array | ? | Debe contener al menos 1 detalle |

**Validaciones de InvoiceDetails:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `productId` | integer | ? | Debe ser mayor a 0, producto debe existir y estar activo |
| `quantity` | integer | ? | Debe ser mayor a 0 |
| `unitPrice` | decimal | ? | Debe ser positivo (> 0) |

**Validaciones Especiales:**

**Cliente** (`clientId`):
- El cliente debe existir en la base de datos
- El cliente debe estar activo (`IsActive = true`)

**Detalles de la Factura** (`invoiceDetails`):
- Validado por `[ValidInvoiceDetails]`
- Debe contener al menos 1 detalle
- Cada producto debe existir y estar activo
- Debe haber stock suficiente para cada producto
- Los precios se validan contra los precios actuales en base de datos

**Cálculos Automáticos:**
- **Subtotal por detalle**: `quantity * unitPrice`
- **Subtotal de factura**: Suma de todos los subtotales de detalles
- **Tax (IVA)**: `subtotal * 0.12` (12% en Ecuador)
- **Total**: `subtotal + tax`

**Número de Factura:**
- Se genera automáticamente con formato: `FAC-{año}-{secuencial}`
- Ejemplo: `FAC-2024-00001`, `FAC-2024-00002`
- El secuencial es correlativo por año

**Usuario Actual:**
- Se obtiene automáticamente del token JWT
- No se envía en el request body

**Respuestas:**

**? 201 Created**
```json
{
  "success": true,
  "message": "Factura creada exitosamente",
  "invoice": {
    "invoiceId": 125,
    "invoiceNumber": "FAC-2024-00125",
    "clientId": 5,
    "userId": "user-guid-1234",
    "issueDate": "2024-06-15T10:30:00Z",
    "subtotal": 1799.98,
    "tax": 215.9976,
    "total": 2015.9776,
    "observations": "Cliente preferencial - descuento del 10%",
    "status": "Borrador",
    "invoiceDetails": [
    {
        "productId": 10,
        "quantity": 2,
        "unitPrice": 750.00,
   "subtotal": 1500.00
      },
      {
        "productId": 15,
   "quantity": 1,
        "unitPrice": 299.99,
   "subtotal": 299.99
      }
  ]
  }
}
```

**? 400 Bad Request** - Validación fallida
```json
{
  "message": "Validation failed",
  "errors": {
    "ClientId": ["El ID del cliente es requerido"],
    "InvoiceDetails": ["La factura debe contener al menos un detalle"]
  }
}
```

**? 400 Bad Request** - Cliente no existe
```json
{
  "message": "El cliente especificado no existe"
}
```

**? 400 Bad Request** - Cliente inactivo
```json
{
  "message": "El cliente está inactivo"
}
```

**? 400 Bad Request** - Producto sin stock
```json
{
  "message": "Stock insuficiente para el producto 'Laptop Dell XPS 15'"
}
```

**? 401 Unauthorized** - Usuario no autenticado
```json
{
  "message": "Usuario no autenticado"
}
```

---

### PUT `/api/Invoice/{id}`
Actualiza una factura existente (solo facturas en estado "Borrador").

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Path Parameters:**
- `id` (integer): ID de la factura

**Request Body:**
```json
{
"invoiceId": 1,
  "clientId": 5,
  "observations": "Observaciones actualizadas",
  "invoiceDetails": [
    {
  "productId": 10,
      "quantity": 3,
      "unitPrice": 750.00
    }
  ]
}
```

**Validaciones:**
- `invoiceId` en el body debe coincidir con `id` de la URL
- Solo se pueden actualizar facturas en estado "Borrador"
- Todas las validaciones de creación aplican
- Los detalles se reemplazan completamente (no se hace merge)

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Factura actualizada exitosamente",
  "invoice": {
    "invoiceId": 1,
    "invoiceNumber": "FAC-2024-00001",
    "clientId": 5,
    "subtotal": 2250.00,
    "tax": 270.00,
    "total": 2520.00,
    "status": "Borrador"
  }
}
```

**? 400 Bad Request** - ID mismatch
```json
{
  "message": "ID mismatch",
  "details": "URL ID (5) does not match body ID (3)"
}
```

**? 400 Bad Request** - Factura no es borrador
```json
{
  "message": "Solo se pueden actualizar facturas en estado Borrador"
}
```

**? 404 Not Found**
```json
{
  "message": "Factura no encontrada"
}
```

---

### DELETE `/api/Invoice/{id}`
Realiza borrado lógico de una factura (cambia `IsActive` a `false`).

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer): ID de la factura

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Invoice deleted successfully (soft delete)"
}
```

**? 404 Not Found**
```json
{
  "message": "Factura no encontrada"
}
```

**? 400 Bad Request**
```json
{
  "message": "La factura ya está eliminada"
}
```

---

## ?? Operaciones Especiales

### POST `/api/Invoice/{id}/finalize`
Finaliza una factura (cambia de "Borrador" a "Finalizada").

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer): ID de la factura

**Descripción:**
- Cambia el estado de la factura de "Borrador" a "Finalizada"
- Reduce el stock de los productos según las cantidades facturadas
- Una vez finalizada, la factura no puede ser modificada
- Es un proceso **irreversible** (excepto cancelación)

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Invoice finalized successfully",
  "invoice": {
    "invoiceId": 1,
    "invoiceNumber": "FAC-2024-00001",
    "status": "Finalizada",
    "issueDate": "2024-06-15T10:30:00Z",
    "total": 1680.00
  }
}
```

**? 400 Bad Request** - Factura no es borrador
```json
{
  "message": "Solo se pueden finalizar facturas en estado Borrador"
}
```

**? 400 Bad Request** - Stock insuficiente
```json
{
  "message": "Stock insuficiente para finalizar la factura"
}
```

**? 404 Not Found**
```json
{
  "message": "Factura no encontrada"
}
```

---

### POST `/api/Invoice/{id}/cancel`
Cancela una factura con una razón específica.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Path Parameters:**
- `id` (integer): ID de la factura

**Request Body:**
```json
{
  "invoiceId": 1,
  "reason": "Cliente solicitó cancelación por error en los productos"
}
```

**Validaciones del Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `invoiceId` | integer | ? | Debe coincidir con ID de la URL |
| `reason` | string | ? | Requerido, máx 200 caracteres |

**Descripción:**
- Cambia el estado de la factura a "Cancelada"
- Registra la razón de cancelación
- Si la factura estaba finalizada, restaura el stock de los productos
- Una vez cancelada, la factura no puede ser reactivada

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Invoice cancelled successfully",
  "invoice": {
    "invoiceId": 1,
    "invoiceNumber": "FAC-2024-00001",
    "status": "Cancelada",
    "cancelReason": "Cliente solicitó cancelación por error en los productos"
  }
}
```

**? 400 Bad Request** - ID mismatch
```json
{
  "message": "ID mismatch"
}
```

**? 400 Bad Request** - Razón no proporcionada
```json
{
  "message": "Validation failed",
"errors": {
    "Reason": ["La razón de cancelación es requerida"]
  }
}
```

**? 400 Bad Request** - Factura ya cancelada
```json
{
  "message": "La factura ya está cancelada"
}
```

**? 404 Not Found**
```json
{
  "message": "Factura no encontrada"
}
```

---

## ?? Búsqueda Avanzada

### POST `/api/Invoice/search`
Búsqueda avanzada de facturas con múltiples filtros.

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "searchTerm": "Juan",
  "fromDate": "2024-01-01T00:00:00Z",
  "toDate": "2024-12-31T23:59:59Z",
  "clientId": 5,
  "status": "Finalizada",
  "includeDeleted": false
}
```

**Parámetros de Búsqueda:**

| Campo | Tipo | Requerido | Descripción |
|-------|------|-----------|-------------|
| `pageNumber` | integer | ? | Número de página (default: 1) |
| `pageSize` | integer | ? | Tamaño de página (default: 10) |
| `searchTerm` | string | ? | Busca en número de factura, nombre de cliente |
| `fromDate` | datetime | ? | Fecha inicial (filtro por fecha de emisión) |
| `toDate` | datetime | ? | Fecha final (filtro por fecha de emisión) |
| `clientId` | integer | ? | Filtrar por cliente específico |
| `status` | string | ? | Filtrar por estado ("Borrador", "Finalizada", "Cancelada") |
| `includeDeleted` | boolean | ? | Incluir facturas eliminadas (default: false) |

**Respuestas:**

**? 200 OK**
```json
{
  "items": [
    {
      "invoiceId": 1,
      "invoiceNumber": "FAC-2024-00001",
 "clientId": 5,
    "issueDate": "2024-06-15T10:30:00Z",
      "total": 1680.00,
      "status": "Finalizada",
    "client": {
"firstName": "Juan",
    "lastName": "Pérez"
      }
    }
  ],
  "totalCount": 15,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

**Ejemplos de Búsqueda:**

**Por rango de fechas:**
```json
{
  "fromDate": "2024-06-01T00:00:00Z",
  "toDate": "2024-06-30T23:59:59Z"
}
```

**Por cliente específico:**
```json
{
  "clientId": 5,
  "status": "Finalizada"
}
```

**Por estado:**
```json
{
  "status": "Borrador"
}
```

---

## ??? Gestión de Borrado Lógico

### GET `/api/Invoice/all-including-deleted`
Obtiene todas las facturas incluyendo eliminadas.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Query Parameters:**
- `pageNumber` (integer, opcional): Número de página (default: 1)
- `pageSize` (integer, opcional): Tamaño de página (default: 10)
- `searchTerm` (string, opcional): Término de búsqueda

**Ejemplo:**
```
GET /api/Invoice/all-including-deleted?pageNumber=1&pageSize=20
```

**Respuestas:**

**? 200 OK**
```json
{
  "items": [
    {
      "invoiceId": 1,
      "invoiceNumber": "FAC-2024-00001",
      "status": "Finalizada",
      "isActive": true,
      "deletedAt": null
    },
 {
   "invoiceId": 2,
      "invoiceNumber": "FAC-2024-00002",
    "status": "Cancelada",
      "isActive": false,
  "deletedAt": "2024-05-20T10:00:00Z"
    }
  ],
  "totalCount": 260
}
```

---

### GET `/api/Invoice/deleted`
Obtiene solo las facturas eliminadas.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Respuestas:**

**? 200 OK**
```json
[
  {
    "invoiceId": 2,
    "invoiceNumber": "FAC-2024-00002",
    "clientId": 8,
    "issueDate": "2024-05-15T14:00:00Z",
    "total": 850.00,
    "status": "Cancelada",
    "isActive": false,
    "deletedAt": "2024-05-20T10:00:00Z"
  }
]
```

---

### POST `/api/Invoice/{id}/restore`
Restaura una factura eliminada lógicamente.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer): ID de la factura

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Invoice restored successfully"
}
```

**? 404 Not Found**
```json
{
  "message": "Factura no encontrada"
}
```

**? 400 Bad Request**
```json
{
  "message": "La factura no está eliminada"
}
```

---

## ?? Detalles de Factura

### GET `/api/Invoice/{id}/details`
Obtiene los detalles completos de una factura específica.

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer): ID de la factura

**Respuestas:**

**? 200 OK**
```json
{
  "invoiceId": 1,
  "invoiceNumber": "FAC-2024-00001",
  "client": {
    "clientId": 5,
    "firstName": "Juan",
"lastName": "Pérez",
    "identificationType": "CEDULA",
    "identificationNumber": "1234567890",
    "email": "juan@example.com",
    "phone": "0991234567"
  },
  "issueDate": "2024-06-15T10:30:00Z",
  "subtotal": 1500.00,
  "tax": 180.00,
  "total": 1680.00,
  "observations": "Cliente preferencial",
  "status": "Finalizada",
  "invoiceDetails": [
    {
      "invoiceDetailId": 1,
  "productId": 10,
  "productCode": "PROD-001",
 "productName": "Laptop Dell XPS 15",
      "quantity": 2,
  "unitPrice": 750.00,
      "subtotal": 1500.00
  }
  ]
}
```

---

## ?? Estadísticas

### GET `/api/Invoice/statistics`
Obtiene estadísticas generales de facturas.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Respuestas:**

**? 200 OK**
```json
{
"activeInvoices": 240,
  "deletedInvoices": 10,
  "totalInvoices": 250,
  "deletionRate": 4.0
}
```

---

## ?? Notas Importantes

### Estados de Factura
- **Borrador**: Factura creada pero no finalizada, puede ser modificada
- **Finalizada**: Factura confirmada, reduce stock, no puede modificarse
- **Cancelada**: Factura anulada, restaura stock si estaba finalizada

### Flujo de Trabajo Típico
1. **Crear** factura en estado "Borrador" ? `POST /api/Invoice`
2. **Modificar** si es necesario ? `PUT /api/Invoice/{id}` (solo si es Borrador)
3. **Finalizar** cuando esté correcta ? `POST /api/Invoice/{id}/finalize`
4. **Cancelar** si hay error ? `POST /api/Invoice/{id}/cancel` (solo Administrator)

### Generación de Número de Factura
- Formato: `FAC-{año}-{secuencial:00000}`
- Ejemplos:
  - `FAC-2024-00001`
  - `FAC-2024-00125`
  - `FAC-2025-00001` (se reinicia cada año)

### Cálculo de IVA
- Ecuador usa IVA del 12%
- Fórmula: `tax = subtotal * 0.12`
- El total incluye IVA: `total = subtotal + tax`

### Gestión de Stock
- **Crear factura**: No afecta el stock (estado Borrador)
- **Finalizar factura**: Reduce el stock de cada producto
- **Cancelar factura finalizada**: Restaura el stock
- **Cancelar factura borrador**: No afecta el stock

### Validaciones de Negocio
- No se puede finalizar una factura sin stock suficiente
- No se puede modificar una factura finalizada
- No se puede cancelar una factura ya cancelada
- No se puede restaurar una factura activa
- El cliente debe estar activo para crear facturas
- Los productos deben estar activos para incluirlos en facturas

### Auditoría
- `createdAt`: Fecha de creación de la factura
- `updatedAt`: Fecha de última modificación
- `deletedAt`: Fecha de eliminación (borrado lógico)
- `userId`: ID del usuario que creó la factura (obtenido del token JWT)

### Búsqueda (searchTerm)
El parámetro `searchTerm` busca en:
- Número de factura (`invoiceNumber`)
- Nombre del cliente (`firstName`, `lastName`)
- Email del cliente

### Permisos
- **Administrator**: Acceso completo a todas las operaciones
- **user**: Puede crear, ver y finalizar facturas, pero no cancelar ni eliminar

### Códigos de Error Comunes
- **400**: Validación fallida, estado inválido o datos incorrectos
- **401**: No autenticado
- **403**: No autorizado (falta permisos)
- **404**: Factura no encontrada
- **500**: Error interno del servidor

### Ejemplo Completo: Flujo de Facturación

**1. Crear Factura (Borrador):**
```json
POST /api/Invoice
{
  "clientId": 5,
  "observations": "Venta especial",
  "invoiceDetails": [
    { "productId": 10, "quantity": 2, "unitPrice": 750.00 }
  ]
}
```
Respuesta: `{ "invoiceNumber": "FAC-2024-00001", "status": "Borrador" }`

**2. Finalizar Factura:**
```json
POST /api/Invoice/1/finalize
```
Respuesta: `{ "message": "Invoice finalized successfully", "status": "Finalizada" }`

**3. Cancelar si es necesario:**
```json
POST /api/Invoice/1/cancel
{
  "invoiceId": 1,
  "reason": "Error en cantidad de productos"
}
```
Respuesta: `{ "message": "Invoice cancelled successfully", "status": "Cancelada" }`
