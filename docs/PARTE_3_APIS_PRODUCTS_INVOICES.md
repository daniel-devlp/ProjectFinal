# ?? DOCUMENTACIÓN DEL PROYECTO - PARTE 3
## DOCUMENTACIÓN DE APIs - PRODUCTOS Y FACTURAS

---

## ?? Índice
1. [Productos (Product)](#1-productos-product)
2. [Facturas (Invoice)](#2-facturas-invoice)

---

## ?? 1. Productos (Product)

### Base URL
```
/api/product
```

---

### 1.1. Obtener Producto por ID

**Endpoint:** `GET /api/product/{id}`

**Descripción:** Obtiene un producto por su ID (solo productos activos).

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID del producto |

**Response Success (200 OK):**
```json
{
  "productId": 1,
  "code": "PROD-001",
  "name": "Laptop Dell Inspiron 15",
  "description": "Laptop de alto rendimiento con procesador Intel i7",
  "price": 899.99,
  "stock": 15,
  "isActive": true,
  "imageUri": "https://res.cloudinary.com/.../laptop.jpg",
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "deletedAt": null
}
```

**Response Errors:**
- **404 Not Found:** Producto no encontrado
- **400 Bad Request:** ID inválido

---

### 1.2. Obtener Todos los Productos

**Endpoint:** `GET /api/product`

**Descripción:** Lista todos los productos activos con paginación y búsqueda.

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Query Parameters:**
| Parámetro | Tipo | Descripción | Default | Requerido |
|-----------|------|-------------|---------|-----------|
| pageNumber | int | Número de página | 1 | No |
| pageSize | int | Registros por página | 10 | No |
| searchTerm | string | Buscar por código o nombre | null | No |

**Ejemplo Request:**
```
GET /api/product?pageNumber=1&pageSize=20&searchTerm=Laptop
```

**Response Success (200 OK):**
```json
{
  "items": [
    {
 "productId": 1,
      "code": "PROD-001",
   "name": "Laptop Dell Inspiron 15",
   "description": "Laptop de alto rendimiento",
  "price": 899.99,
      "stock": 15,
      "isActive": true,
      "imageUri": "https://res.cloudinary.com/.../laptop.jpg",
   "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "firstItemIndex": 1,
  "lastItemIndex": 20,
"isEmpty": false
}
```

---

### 1.3. Obtener Productos Incluyendo Eliminados

**Endpoint:** `GET /api/product/all-including-deleted`

**Descripción:** Lista todos los productos incluyendo los eliminados lógicamente.

**Autorización:** ? Requiere rol `Administrator`

**Query Parameters:**
| Parámetro | Tipo | Descripción | Default | Requerido |
|-----------|------|-------------|---------|-----------|
| pageNumber | int | Número de página | 1 | No |
| pageSize | int | Registros por página | 10 | No |
| searchTerm | string | Término de búsqueda | null | No |

---

### 1.4. Obtener Solo Productos Eliminados

**Endpoint:** `GET /api/product/deleted`

**Descripción:** Lista solo los productos eliminados lógicamente.

**Autorización:** ? Requiere rol `Administrator`

**Response Success (200 OK):**
```json
[
  {
    "productId": 10,
    "code": "PROD-010",
    "name": "Producto Descontinuado",
  "isActive": false,
"deletedAt": "2024-06-01T10:30:00Z"
  }
]
```

---

### 1.5. Obtener Productos con Stock Bajo

**Endpoint:** `GET /api/product/low-stock`

**Descripción:** Obtiene productos con stock bajo según un umbral.

**Autorización:** ? Requiere rol `Administrator`

**Query Parameters:**
| Parámetro | Tipo | Descripción | Default | Requerido |
|-----------|------|-------------|---------|-----------|
| threshold | int | Umbral de stock bajo | 10 | No |

**Ejemplo Request:**
```
GET /api/product/low-stock?threshold=5
```

**Response Success (200 OK):**
```json
[
  {
    "productId": 3,
    "code": "PROD-003",
    "name": "Mouse Inalámbrico",
    "stock": 3,
    "price": 15.99
  },
  {
    "productId": 7,
    "code": "PROD-007",
    "name": "Teclado Mecánico",
  "stock": 2,
    "price": 79.99
  }
]
```

---

### 1.6. Crear Producto

**Endpoint:** `POST /api/product`

**Descripción:** Crea un nuevo producto con validaciones completas y opción de imagen.

**Autorización:** ? Requiere rol `Administrator`

**Content-Type:** `multipart/form-data`

**Request Body (Form Data):**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| code | string | Código único del producto | Requerido, formato: letras mayúsculas, números, guiones |
| name | string | Nombre del producto | Requerido, 2-200 caracteres |
| description | string | Descripción | Opcional, máx 1000 caracteres |
| price | decimal | Precio unitario | Requerido, debe ser positivo |
| stock | int | Cantidad en inventario | Requerido, no negativo (?0) |
| image | file | Imagen del producto | Opcional, formatos: jpg, png, gif |

**Validación de Código de Producto:**
- Solo letras mayúsculas, números y guiones
- Ejemplo: `PROD-001`, `LAP-DELL-2024`, `KB123`

**Ejemplo Request:**
```
code: PROD-100
name: Monitor LG 27 pulgadas
description: Monitor Full HD con tecnología IPS
price: 299.99
stock: 20
image: [archivo binario]
```

**Response Success (201 Created):**
```json
{
  "success": true,
  "message": "Product created successfully with image",
  "product": {
 "code": "PROD-100",
    "name": "Monitor LG 27 pulgadas",
    "description": "Monitor Full HD con tecnología IPS",
    "price": 299.99,
    "stock": 20,
    "imageUrl": "https://res.cloudinary.com/.../monitor.jpg"
  }
}
```

**Response Errors:**

**400 Bad Request - Validación fallida:**
```json
{
  "message": "Validation failed",
  "errors": {
"Code": [
      "El código solo puede contener letras mayúsculas, números y guiones"
    ],
    "Price": [
      "El precio debe ser mayor a cero"
    ],
    "Stock": [
      "El stock no puede ser negativo"
    ]
  }
}
```

**400 Bad Request - Código duplicado:**
```json
{
  "message": "Ya existe un producto con este código"
}
```

**400 Bad Request - Error al subir imagen:**
```json
{
  "success": false,
  "message": "Failed to upload image",
  "error": "Formato de archivo no soportado"
}
```

---

### 1.7. Actualizar Producto

**Endpoint:** `PUT /api/product/{id}`

**Descripción:** Actualiza un producto existente con validaciones.

**Autorización:** ? Requiere rol `Administrator`

**Content-Type:** `multipart/form-data`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID del producto |

**Request Body (Form Data):**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| code | string | Código del producto | Requerido |
| name | string | Nombre del producto | Requerido, 2-200 caracteres |
| description | string | Descripción | Opcional, máx 1000 caracteres |
| price | decimal | Precio unitario | Requerido, positivo |
| stock | int | Cantidad en inventario | Requerido, no negativo |
| isActive | boolean | Estado del producto | Opcional, default: true |
| image | file | Nueva imagen | Opcional |

**Ejemplo Request:**
```
code: PROD-100
name: Monitor LG 27" Ultra HD
description: Monitor 4K con HDR
price: 399.99
stock: 15
isActive: true
image: [nueva imagen]
```

**?? Comportamiento de Imagen:**
- Si se proporciona `image`, se sube la nueva y se elimina la anterior
- Si NO se proporciona `image`, se mantiene la imagen existente

**Response Success (200 OK):**
```json
{
  "message": "Product updated successfully with new image",
  "product": {
    "id": 1,
    "code": "PROD-100",
 "name": "Monitor LG 27\" Ultra HD",
    "imageUrl": "https://res.cloudinary.com/.../monitor-new.jpg"
  }
}
```

**Response Errors:**
- **400 Bad Request:** Validación fallida
- **404 Not Found:** Producto no encontrado

---

### 1.8. Eliminar Producto (Soft Delete)

**Endpoint:** `DELETE /api/product/{id}`

**Descripción:** Realiza borrado lógico del producto (cambia `isActive` a `false`).

**Autorización:** ? Requiere rol `Administrator`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID del producto |

**Response Success (200 OK):**
```json
{
  "message": "Product deleted successfully (soft delete)"
}
```

?? **Nota:** El producto no se elimina físicamente de la base de datos. Solo se marca como inactivo.

---

### 1.9. Restaurar Producto

**Endpoint:** `POST /api/product/{id}/restore`

**Descripción:** Restaura un producto eliminado lógicamente.

**Autorización:** ? Requiere rol `Administrator`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID del producto |

**Response Success (200 OK):**
```json
{
  "message": "Product restored successfully"
}
```

---

### 1.10. Obtener Estadísticas de Productos

**Endpoint:** `GET /api/product/statistics`

**Descripción:** Obtiene estadísticas generales de productos.

**Autorización:** ? Requiere rol `Administrator`

**Response Success (200 OK):**
```json
{
  "activeProducts": 95,
  "deletedProducts": 5,
  "totalProducts": 100,
  "deletionRate": 5.0
}
```

---

### 1.11. Buscar Producto por Código

**Endpoint:** `GET /api/product/by-code/{code}`

**Descripción:** Busca un producto por su código único.

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| code | string | Código del producto |

**Ejemplo Request:**
```
GET /api/product/by-code/PROD-100
```

**Response Success (200 OK):**
```json
{
  "productId": 1,
  "code": "PROD-100",
  "name": "Monitor LG 27 pulgadas",
  "description": "Monitor Full HD",
  "price": 299.99,
  "stock": 20,
  "imageUri": "https://res.cloudinary.com/.../monitor.jpg"
}
```

**Response Errors:**
- **404 Not Found:** Producto no encontrado

---

### 1.12. Actualizar Stock de Producto

**Endpoint:** `PATCH /api/product/{id}/stock`

**Descripción:** Actualiza solo el stock de un producto.

**Autorización:** ? Requiere rol `Administrator`

**Content-Type:** `application/json`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID del producto |

**Request Body:**
```json
{
  "productId": 1,
  "quantity": 50,
  "reason": "Reabastecimiento mensual"
}
```

**Parámetros Requeridos:**
| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| productId | int | ID del producto | Requerido, debe coincidir con URL |
| quantity | int | Nueva cantidad de stock | Requerido, mayor a 0 |
| reason | string | Motivo de actualización | Opcional |

**?? Importante:** El `productId` en el body debe coincidir con el `id` de la URL.

**Response Success (200 OK):**
```json
{
  "message": "Stock updated successfully"
}
```

**Response Errors:**
- **400 Bad Request:** ID mismatch o cantidad inválida
- **404 Not Found:** Producto no encontrado

---

## ?? 2. Facturas (Invoice)

### Base URL
```
/api/invoice
```

---

### 2.1. Obtener Factura por ID

**Endpoint:** `GET /api/invoice/{id}`

**Descripción:** Obtiene una factura por su ID (solo facturas activas).

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID de la factura |

**Response Success (200 OK):**
```json
{
  "invoiceId": 1,
  "invoiceNumber": "FAC-2024-00001",
  "clientId": 5,
  "userId": "guid-user",
  "issueDate": "2024-01-15T10:30:00Z",
  "subtotal": 1000.00,
  "tax": 120.00,
  "total": 1120.00,
  "observations": "Pago al contado",
  "isActive": true,
  "status": "Finalized",
  "cancelReason": null,
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": null,
  "deletedAt": null,
  "client": {
    "clientId": 5,
    "firstName": "Juan",
    "lastName": "Pérez",
    "identificationNumber": "1713175071"
  },
  "invoiceDetails": [
    {
      "invoiceDetailId": 1,
      "productId": 10,
      "productName": "Laptop Dell",
      "quantity": 2,
    "unitPrice": 500.00,
      "subtotal": 1000.00
    }
  ]
}
```

**Response Errors:**
- **404 Not Found:** Factura no encontrada
- **400 Bad Request:** ID inválido

---

### 2.2. Obtener Todas las Facturas

**Endpoint:** `GET /api/invoice`

**Descripción:** Lista todas las facturas activas con paginación y búsqueda.

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Query Parameters:**
| Parámetro | Tipo | Descripción | Default | Requerido |
|-----------|------|-------------|---------|-----------|
| pageNumber | int | Número de página | 1 | No |
| pageSize | int | Registros por página | 10 | No |
| searchTerm | string | Buscar por número de factura | null | No |

**Ejemplo Request:**
```
GET /api/invoice?pageNumber=1&pageSize=20&searchTerm=FAC-2024
```

**Response Success (200 OK):**
```json
{
  "items": [
    {
      "invoiceId": 1,
      "invoiceNumber": "FAC-2024-00001",
 "clientId": 5,
  "issueDate": "2024-01-15T10:30:00Z",
   "total": 1120.00,
      "status": "Finalized",
   "client": {
        "firstName": "Juan",
      "lastName": "Pérez"
      }
    }
  ],
  "totalCount": 50,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

### 2.3. Obtener Facturas Incluyendo Eliminadas

**Endpoint:** `GET /api/invoice/all-including-deleted`

**Descripción:** Lista todas las facturas incluyendo las eliminadas lógicamente.

**Autorización:** ? Requiere rol `Administrator`

**Query Parameters:**
| Parámetro | Tipo | Descripción | Default | Requerido |
|-----------|------|-------------|---------|-----------|
| pageNumber | int | Número de página | 1 | No |
| pageSize | int | Registros por página | 10 | No |
| searchTerm | string | Término de búsqueda | null | No |

---

### 2.4. Obtener Solo Facturas Eliminadas

**Endpoint:** `GET /api/invoice/deleted`

**Descripción:** Lista solo las facturas eliminadas lógicamente.

**Autorización:** ? Requiere rol `Administrator`

**Response Success (200 OK):**
```json
[
  {
    "invoiceId": 15,
 "invoiceNumber": "FAC-2024-00015",
    "total": 500.00,
    "isActive": false,
    "deletedAt": "2024-06-01T10:30:00Z"
  }
]
```

---

### 2.5. Crear Factura

**Endpoint:** `POST /api/invoice`

**Descripción:** Crea una nueva factura con validaciones completas y número automático.

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "clientId": 5,
  "observations": "Pago al contado con descuento",
  "invoiceDetails": [
    {
      "productId": 10,
      "quantity": 2,
"unitPrice": 500.00
    },
    {
      "productId": 15,
      "quantity": 1,
   "unitPrice": 120.00
    }
  ]
}
```

**Parámetros Requeridos:**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| clientId | int | ID del cliente | Requerido, mayor a 0 |
| observations | string | Observaciones | Opcional, máx 500 caracteres |
| invoiceDetails | array | Detalles de productos | Requerido, al menos 1 item |

**Estructura de InvoiceDetails:**
| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| productId | int | ID del producto | Requerido, mayor a 0 |
| quantity | int | Cantidad | Requerido, mayor a 0 |
| unitPrice | decimal | Precio unitario | Requerido, mayor a 0 |

**?? Validaciones Automáticas:**
- El número de factura se genera automáticamente (formato: `FAC-YYYY-NNNNN`)
- Se valida que el cliente exista y esté activo
- Se valida que los productos existan y tengan stock suficiente
- Se calcula automáticamente: subtotal, impuestos (12% IVA) y total
- El usuario se obtiene del token JWT automáticamente
- El estado inicial es `Draft` (Borrador)

**Ejemplo de Cálculos:**
```
Producto 1: 2 × $500.00 = $1000.00
Producto 2: 1 × $120.00 = $120.00
Subtotal: $1120.00
IVA (12%): $134.40
Total: $1254.40
```

**Response Success (201 Created):**
```json
{
"success": true,
  "message": "Factura creada exitosamente",
  "invoice": {
    "invoiceId": 25,
    "invoiceNumber": "FAC-2024-00025",
    "clientId": 5,
    "userId": "current-user-guid",
    "issueDate": "2024-01-15T10:30:00Z",
    "subtotal": 1120.00,
    "tax": 134.40,
    "total": 1254.40,
    "status": "Draft",
    "client": {
   "firstName": "Juan",
      "lastName": "Pérez"
    },
    "invoiceDetails": [
      {
        "productId": 10,
        "productName": "Laptop Dell",
        "quantity": 2,
        "unitPrice": 500.00,
        "subtotal": 1000.00
      },
      {
    "productId": 15,
        "productName": "Mouse Inalámbrico",
        "quantity": 1,
      "unitPrice": 120.00,
        "subtotal": 120.00
      }
    ]
  }
}
```

**Response Errors:**

**400 Bad Request - Validación fallida:**
```json
{
  "message": "Validation failed",
  "errors": {
    "ClientId": [
      "El ID del cliente debe ser mayor a cero"
    ],
    "InvoiceDetails": [
      "La factura debe contener al menos un detalle"
  ]
  }
}
```

**400 Bad Request - Cliente no existe:**
```json
{
  "message": "El cliente no existe o está inactivo"
}
```

**400 Bad Request - Stock insuficiente:**
```json
{
  "message": "Stock insuficiente para el producto 'Laptop Dell'. Disponible: 1, Solicitado: 2"
}
```

**401 Unauthorized - Usuario no autenticado:**
```json
{
  "message": "Usuario no autenticado"
}
```

---

### 2.6. Actualizar Factura

**Endpoint:** `PUT /api/invoice/{id}`

**Descripción:** Actualiza una factura existente (solo en estado Draft).

**Autorización:** ? Requiere rol `Administrator`

**Content-Type:** `application/json`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID de la factura |

**Request Body:**
```json
{
  "invoiceId": 25,
  "clientId": 5,
  "observations": "Observaciones actualizadas",
  "invoiceDetails": [
    {
      "productId": 10,
      "quantity": 3,
      "unitPrice": 500.00
    }
  ]
}
```

**?? Importante:** 
- El `invoiceId` en el body debe coincidir con el `id` de la URL
- Solo se pueden actualizar facturas en estado `Draft`

**Response Success (200 OK):**
```json
{
  "message": "Factura actualizada exitosamente",
  "invoice": {
  "invoiceId": 25,
    "invoiceNumber": "FAC-2024-00025",
    "subtotal": 1500.00,
    "tax": 180.00,
    "total": 1680.00,
    "updatedAt": "2024-01-15T11:00:00Z"
  }
}
```

**Response Errors:**
- **400 Bad Request:** ID mismatch, factura finalizada/cancelada
- **404 Not Found:** Factura no encontrada

---

### 2.7. Eliminar Factura (Soft Delete)

**Endpoint:** `DELETE /api/invoice/{id}`

**Descripción:** Realiza borrado lógico de la factura.

**Autorización:** ? Requiere rol `Administrator`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID de la factura |

**Response Success (200 OK):**
```json
{
  "message": "Invoice deleted successfully (soft delete)"
}
```

---

### 2.8. Restaurar Factura

**Endpoint:** `POST /api/invoice/{id}/restore`

**Descripción:** Restaura una factura eliminada lógicamente.

**Autorización:** ? Requiere rol `Administrator`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID de la factura |

**Response Success (200 OK):**
```json
{
  "message": "Invoice restored successfully"
}
```

---

### 2.9. Finalizar Factura

**Endpoint:** `POST /api/invoice/{id}/finalize`

**Descripción:** Cambia el estado de la factura de `Draft` a `Finalized`.

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID de la factura |

**?? Validaciones:**
- Solo se pueden finalizar facturas en estado `Draft`
- Una vez finalizada, no se puede editar ni eliminar

**Response Success (200 OK):**
```json
{
  "message": "Invoice finalized successfully",
  "invoice": {
"invoiceId": 25,
    "invoiceNumber": "FAC-2024-00025",
    "status": "Finalized",
    "updatedAt": "2024-01-15T12:00:00Z"
  }
}
```

**Response Errors:**
- **400 Bad Request:** Factura ya está finalizada o cancelada

---

### 2.10. Cancelar Factura

**Endpoint:** `POST /api/invoice/{id}/cancel`

**Descripción:** Cancela una factura con una razón específica.

**Autorización:** ? Requiere rol `Administrator`

**Content-Type:** `application/json`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID de la factura |

**Request Body:**
```json
{
  "invoiceId": 25,
  "reason": "Cliente solicitó cancelación por error en pedido"
}
```

**Parámetros Requeridos:**
| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| invoiceId | int | ID de la factura | Requerido, debe coincidir con URL |
| reason | string | Razón de cancelación | Requerido, máx 200 caracteres |

**Response Success (200 OK):**
```json
{
  "message": "Invoice cancelled successfully",
  "invoice": {
    "invoiceId": 25,
    "invoiceNumber": "FAC-2024-00025",
    "status": "Cancelled",
    "cancelReason": "Cliente solicitó cancelación por error en pedido",
    "updatedAt": "2024-01-15T13:00:00Z"
  }
}
```

**Response Errors:**
- **400 Bad Request:** ID mismatch, factura ya cancelada

---

### 2.11. Búsqueda Avanzada de Facturas

**Endpoint:** `POST /api/invoice/search`

**Descripción:** Búsqueda avanzada con múltiples filtros.

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "pageNumber": 1,
  "pageSize": 20,
  "searchTerm": "FAC-2024",
  "fromDate": "2024-01-01T00:00:00Z",
  "toDate": "2024-12-31T23:59:59Z",
  "clientId": 5,
  "status": "Finalized",
  "includeDeleted": false
}
```

**Parámetros de Búsqueda:**
| Campo | Tipo | Descripción | Default | Requerido |
|-------|------|-------------|---------|-----------|
| pageNumber | int | Número de página | 1 | No |
| pageSize | int | Registros por página | 10 | No |
| searchTerm | string | Buscar en número de factura | null | No |
| fromDate | datetime | Fecha desde | null | No |
| toDate | datetime | Fecha hasta | null | No |
| clientId | int | Filtrar por cliente | null | No |
| status | string | Estado de factura | null | No |
| includeDeleted | boolean | Incluir eliminadas | false | No |

**Valores de Status:**
- `Draft` - Borrador
- `Finalized` - Finalizada
- `Cancelled` - Cancelada

**Response Success (200 OK):**
```json
{
  "items": [
    {
      "invoiceId": 25,
    "invoiceNumber": "FAC-2024-00025",
      "issueDate": "2024-01-15T10:30:00Z",
      "clientId": 5,
      "total": 1254.40,
      "status": "Finalized"
    }
  ],
  "totalCount": 15,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 1
}
```

---

### 2.12. Obtener Estadísticas de Facturas

**Endpoint:** `GET /api/invoice/statistics`

**Descripción:** Obtiene estadísticas generales de facturas.

**Autorización:** ? Requiere rol `Administrator`

**Response Success (200 OK):**
```json
{
  "activeInvoices": 45,
  "deletedInvoices": 5,
  "totalInvoices": 50,
  "deletionRate": 10.0
}
```

---

### 2.13. Obtener Detalles de Factura

**Endpoint:** `GET /api/invoice/{id}/details`

**Descripción:** Obtiene los detalles completos de productos de una factura.

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID de la factura |

**Response Success (200 OK):**
```json
[
  {
 "invoiceDetailId": 1,
    "invoiceId": 25,
    "productId": 10,
    "productName": "Laptop Dell Inspiron 15",
    "productCode": "PROD-010",
    "quantity": 2,
    "unitPrice": 500.00,
  "subtotal": 1000.00
  },
  {
    "invoiceDetailId": 2,
    "invoiceId": 25,
    "productId": 15,
  "productName": "Mouse Inalámbrico",
    "productCode": "PROD-015",
    "quantity": 1,
    "unitPrice": 120.00,
    "subtotal": 120.00
  }
]
```

---

**Continúa en PARTE 4...**

---

**Fecha de última actualización:** $(Get-Date)
**Versión del proyecto:** .NET 9
