# ?? DOCUMENTACIÓN DEL PROYECTO - PARTE 4
## DOCUMENTACIÓN DE APIs - PAGOS, CARRITO Y SERVICIOS ADICIONALES

---

## ?? Índice
1. [Pagos (Payment)](#1-pagos-payment)
2. [Métodos de Pago (PaymentMethod)](#2-métodos-de-pago-paymentmethod)
3. [Carrito de Compras (ShoppingCart)](#3-carrito-de-compras-shoppingcart)
4. [Imágenes (Images)](#4-imágenes-images)
5. [Roles](#5-roles)
6. [Códigos de Estado HTTP](#6-códigos-de-estado-http)
7. [Modelo de Datos Completo](#7-modelo-de-datos-completo)
8. [Ejemplos de Integración](#8-ejemplos-de-integración)

---

## ?? 1. Pagos (Payment)

### Base URL
```
/api/payment
```

**Autorización General:** ? Todos los endpoints requieren autenticación

---

### 1.1. Procesar Pago

**Endpoint:** `POST /api/payment/process`

**Descripción:** Procesa un pago con simulación (ideal para aplicaciones móviles).

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "invoiceId": 25,
  "paymentMethodId": "credit_card",
  "amount": 1254.40,
  "additionalInfo": "Pago con tarjeta Visa ****1234"
}
```

**Parámetros Requeridos:**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| invoiceId | int | ID de la factura | Requerido, mayor a 0 |
| paymentMethodId | string | ID del método de pago | Requerido, máx 50 caracteres |
| amount | decimal | Monto a pagar | Requerido, mayor a 0 |
| additionalInfo | string | Información adicional | Opcional, máx 1000 caracteres |

**?? Validaciones Automáticas:**
- Se valida que la factura exista y esté finalizada
- Se valida que el monto coincida con el total de la factura
- Se valida que el método de pago esté activo
- Se genera un `TransactionId` único automáticamente

**Response Success (200 OK):**
```json
{
  "success": true,
  "transactionId": "TXN-2024-ABC123",
  "status": "Completed",
  "message": "Payment processed successfully",
  "payment": {
    "paymentId": 15,
    "invoiceId": 25,
    "paymentMethodId": "credit_card",
    "paymentMethodName": "Tarjeta de Crédito",
    "amount": 1254.40,
    "transactionId": "TXN-2024-ABC123",
    "status": "Completed",
    "paymentDate": "2024-01-15T14:30:00Z",
    "processedAt": "2024-01-15T14:30:05Z",
    "processorResponse": "Transaction approved",
    "failureReason": null,
    "createdAt": "2024-01-15T14:30:00Z",
    "updatedAt": "2024-01-15T14:30:05Z"
  }
}
```

**Response Error (400 Bad Request):**
```json
{
  "success": false,
  "transactionId": null,
  "status": "Failed",
  "message": "Insufficient funds or invalid card",
  "errorCode": "PAYMENT_ERROR",
  "payment": null
}
```

**Posibles Estados de Pago:**
- `Pending` - Pendiente
- `Completed` - Completado
- `Failed` - Fallido
- `Cancelled` - Cancelado
- `Refunded` - Reembolsado

---

### 1.2. Procesar Pago Móvil

**Endpoint:** `POST /api/payment/mobile`

**Descripción:** Procesa un pago específico para dispositivos móviles.

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "invoiceId": 25,
  "paymentMethodId": "mobile_wallet",
  "amount": 1254.40,
  "deviceId": "DEVICE-ABC123",
  "appVersion": "1.0.0",
  "platform": "Android"
}
```

**Parámetros Requeridos:**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| invoiceId | int | ID de la factura | Requerido |
| paymentMethodId | string | ID del método de pago | Requerido |
| amount | decimal | Monto a pagar | Requerido, mayor a 0 |
| deviceId | string | ID del dispositivo | Opcional |
| appVersion | string | Versión de la app | Opcional |
| platform | string | Plataforma (Android/iOS) | Opcional |

**Response Success (200 OK):**
```json
{
  "success": true,
  "transactionId": "TXN-MOBILE-2024-XYZ789",
  "status": "Completed",
  "message": "Mobile payment processed successfully",
  "payment": {
    "paymentId": 16,
  "invoiceId": 25,
    "paymentMethodId": "mobile_wallet",
    "paymentMethodName": "Billetera Móvil",
    "amount": 1254.40,
    "transactionId": "TXN-MOBILE-2024-XYZ789",
    "status": "Completed",
    "paymentDate": "2024-01-15T14:35:00Z"
  }
}
```

---

### 1.3. Obtener Estado de Pago

**Endpoint:** `GET /api/payment/status/{transactionId}`

**Descripción:** Consulta el estado actual de un pago mediante su Transaction ID.

**Autorización:** ? Requiere autenticación

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| transactionId | string | Transaction ID del pago |

**Ejemplo Request:**
```
GET /api/payment/status/TXN-2024-ABC123
```

**Response Success (200 OK):**
```json
{
  "success": true,
  "transactionId": "TXN-2024-ABC123",
  "status": "Completed",
  "payment": {
"paymentId": 15,
    "invoiceId": 25,
    "amount": 1254.40,
    "status": "Completed",
    "paymentDate": "2024-01-15T14:30:00Z",
    "processedAt": "2024-01-15T14:30:05Z"
  }
}
```

**Response Errors:**
- **404 Not Found:** Pago no encontrado

---

### 1.4. Obtener Métodos de Pago Disponibles

**Endpoint:** `GET /api/payment/methods`

**Descripción:** Lista todos los métodos de pago disponibles y activos.

**Autorización:** ? Requiere autenticación

**Response Success (200 OK):**
```json
{
  "message": "Payment methods retrieved successfully",
  "methods": [
    {
      "paymentMethodId": "credit_card",
      "name": "Tarjeta de Crédito",
 "description": "Visa, Mastercard, American Express",
      "isActive": true,
    "type": "Card",
      "minAmount": 10.00,
      "maxAmount": 50000.00,
      "processingFee": 2.5,
"iconUrl": "https://cdn.example.com/credit-card.png",
      "displayOrder": 1,
    "createdAt": "2024-01-01T00:00:00Z"
 },
    {
      "paymentMethodId": "debit_card",
      "name": "Tarjeta de Débito",
      "description": "Débito bancario",
    "isActive": true,
"type": "Card",
      "minAmount": 5.00,
      "maxAmount": 10000.00,
      "processingFee": 1.5,
      "iconUrl": "https://cdn.example.com/debit-card.png",
      "displayOrder": 2
    },
    {
      "paymentMethodId": "mobile_wallet",
      "name": "Billetera Móvil",
      "description": "Pago mediante app móvil",
      "isActive": true,
      "type": "Digital",
      "minAmount": 1.00,
    "maxAmount": 5000.00,
      "processingFee": 0.5,
      "iconUrl": "https://cdn.example.com/wallet.png",
      "displayOrder": 3
    },
    {
      "paymentMethodId": "bank_transfer",
    "name": "Transferencia Bancaria",
      "description": "Transferencia electrónica",
      "isActive": true,
      "type": "Transfer",
      "minAmount": 50.00,
      "maxAmount": 100000.00,
      "processingFee": 0.0,
      "iconUrl": "https://cdn.example.com/bank.png",
      "displayOrder": 4
    }
  ]
}
```

**Tipos de Métodos de Pago:**
- `Card` - Tarjetas de crédito/débito
- `Digital` - Billeteras digitales
- `Transfer` - Transferencias bancarias
- `Cash` - Efectivo

---

### 1.5. Obtener Historial de Pagos

**Endpoint:** `GET /api/payment/history`

**Descripción:** Obtiene el historial completo de pagos del usuario autenticado.

**Autorización:** ? Requiere autenticación

**Response Success (200 OK):**
```json
{
  "payments": [
    {
      "paymentId": 15,
      "invoiceId": 25,
      "paymentMethodName": "Tarjeta de Crédito",
      "amount": 1254.40,
   "transactionId": "TXN-2024-ABC123",
      "status": "Completed",
      "paymentDate": "2024-01-15T14:30:00Z"
    },
    {
      "paymentId": 12,
      "invoiceId": 20,
      "paymentMethodName": "Billetera Móvil",
      "amount": 789.50,
      "transactionId": "TXN-2024-XYZ456",
      "status": "Completed",
      "paymentDate": "2024-01-10T10:15:00Z"
    }
  ],
  "totalPaid": 2043.90,
  "totalTransactions": 2,
  "lastPaymentDate": "2024-01-15T14:30:00Z"
}
```

---

### 1.6. Cancelar Pago

**Endpoint:** `POST /api/payment/{paymentId}/cancel`

**Descripción:** Cancela un pago pendiente.

**Autorización:** ? Requiere autenticación

**Content-Type:** `application/json`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| paymentId | int | ID del pago |

**Request Body:**
```json
{
  "reason": "Cliente decidió no continuar con la compra"
}
```

**Response Success (200 OK):**
```json
{
  "message": "Payment cancelled successfully",
  "payment": {
    "paymentId": 15,
    "status": "Cancelled",
    "failureReason": "Cliente decidió no continuar con la compra",
    "updatedAt": "2024-01-15T15:00:00Z"
  }
}
```

---

### 1.7. Procesar Reembolso

**Endpoint:** `POST /api/payment/refund`

**Descripción:** Procesa un reembolso de pago (solo administradores).

**Autorización:** ? Requiere rol `Administrator`

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "paymentId": 15,
  "refundAmount": 1254.40,
  "reason": "Producto defectuoso - reembolso completo"
}
```

**Parámetros Requeridos:**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| paymentId | int | ID del pago | Requerido, mayor a 0 |
| refundAmount | decimal | Monto a reembolsar | Requerido, no mayor al monto original |
| reason | string | Razón del reembolso | Requerido, máx 500 caracteres |

**Response Success (200 OK):**
```json
{
  "message": "Refund processed successfully",
  "payment": {
    "paymentId": 15,
    "status": "Refunded",
    "amount": 1254.40,
    "failureReason": "Producto defectuoso - reembolso completo",
    "updatedAt": "2024-01-16T09:00:00Z"
  }
}
```

---

### 1.8. Validar Monto de Pago

**Endpoint:** `POST /api/payment/validate-amount`

**Descripción:** Valida que el monto proporcionado coincida con el total de la factura.

**Autorización:** ? Requiere autenticación

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "invoiceId": 25,
  "amount": 1254.40
}
```

**Response Success (200 OK):**
```json
{
  "isValid": true
}
```

**Response cuando el monto no coincide:**
```json
{
  "isValid": false
}
```

---

## ?? 2. Métodos de Pago (PaymentMethod)

### Base URL
```
/api/paymentmethod
```

### 2.1. Obtener Todos los Métodos de Pago

**Endpoint:** `GET /api/paymentmethod`

**Descripción:** Lista todos los métodos de pago (activos e inactivos).

**Autorización:** ? Requiere rol `Administrator`

---

### 2.2. Crear Método de Pago

**Endpoint:** `POST /api/paymentmethod`

**Descripción:** Crea un nuevo método de pago.

**Autorización:** ? Requiere rol `Administrator`

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "name": "PayPal",
  "description": "Pagos mediante cuenta PayPal",
  "type": "Digital",
  "minAmount": 5.00,
  "maxAmount": 10000.00,
  "processingFee": 3.0,
  "iconUrl": "https://cdn.example.com/paypal.png",
  "isActive": true,
  "displayOrder": 5
}
```

---

### 2.3. Actualizar Método de Pago

**Endpoint:** `PUT /api/paymentmethod/{id}`

**Descripción:** Actualiza un método de pago existente.

**Autorización:** ? Requiere rol `Administrator`

---

### 2.4. Eliminar Método de Pago

**Endpoint:** `DELETE /api/paymentmethod/{id}`

**Descripción:** Desactiva un método de pago (soft delete).

**Autorización:** ? Requiere rol `Administrator`

---

## ??? 3. Carrito de Compras (ShoppingCart)

### Base URL
```
/api/shoppingcart
```

**Autorización General:** ? Todos los endpoints requieren autenticación

---

### 3.1. Obtener Mi Carrito

**Endpoint:** `GET /api/shoppingcart/my-cart`

**Descripción:** Obtiene el carrito activo del usuario autenticado.

**Autorización:** ? Requiere autenticación

**Response Success (200 OK):**
```json
{
  "shoppingCartId": 10,
  "userId": "current-user-guid",
  "items": [
    {
      "shoppingCartItemId": 25,
      "productId": 15,
      "productName": "Laptop Dell Inspiron",
      "productCode": "PROD-015",
      "productImageUri": "https://res.cloudinary.com/.../laptop.jpg",
      "quantity": 2,
      "unitPrice": 899.99,
      "subtotal": 1799.98
    },
    {
  "shoppingCartItemId": 26,
      "productId": 20,
      "productName": "Mouse Inalámbrico",
      "productCode": "PROD-020",
      "quantity": 1,
      "unitPrice": 25.50,
   "subtotal": 25.50
    }
  ],
  "totalItems": 3,
  "subtotal": 1825.48,
  "tax": 219.06,
  "total": 2044.54,
  "createdAt": "2024-01-10T08:00:00Z",
  "updatedAt": "2024-01-15T10:30:00Z"
}
```

---

### 3.2. Agregar Producto al Carrito

**Endpoint:** `POST /api/shoppingcart/add-item`

**Descripción:** Agrega un producto al carrito o incrementa su cantidad.

**Autorización:** ? Requiere autenticación

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "productId": 15,
  "quantity": 2
}
```

**Parámetros Requeridos:**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| productId | int | ID del producto | Requerido, mayor a 0 |
| quantity | int | Cantidad a agregar | Requerido, mayor a 0 |

**?? Validaciones Automáticas:**
- Se valida que el producto exista y esté activo
- Se valida que haya stock suficiente
- Si el producto ya está en el carrito, se incrementa la cantidad
- Si no hay carrito activo, se crea uno nuevo

**Response Success (200 OK):**
```json
{
  "message": "Product added to cart successfully",
  "cart": {
    "shoppingCartId": 10,
    "totalItems": 5,
    "total": 2044.54
  }
}
```

**Response Errors:**

**400 Bad Request - Stock insuficiente:**
```json
{
  "message": "Stock insuficiente. Disponible: 1, Solicitado: 2"
}
```

**404 Not Found - Producto no existe:**
```json
{
  "message": "El producto no existe o está inactivo"
}
```

---

### 3.3. Actualizar Cantidad en Carrito

**Endpoint:** `PUT /api/shoppingcart/update-item/{itemId}`

**Descripción:** Actualiza la cantidad de un producto en el carrito.

**Autorización:** ? Requiere autenticación

**Content-Type:** `application/json`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| itemId | int | ID del item en el carrito |

**Request Body:**
```json
{
  "quantity": 5
}
```

**Response Success (200 OK):**
```json
{
  "message": "Cart item updated successfully",
  "cart": {
    "shoppingCartId": 10,
    "totalItems": 6,
    "total": 4524.95
  }
}
```

---

### 3.4. Eliminar Producto del Carrito

**Endpoint:** `DELETE /api/shoppingcart/remove-item/{itemId}`

**Descripción:** Elimina un producto del carrito.

**Autorización:** ? Requiere autenticación

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| itemId | int | ID del item en el carrito |

**Response Success (200 OK):**
```json
{
  "message": "Item removed from cart successfully",
  "cart": {
    "shoppingCartId": 10,
    "totalItems": 2,
    "total": 1825.48
  }
}
```

---

### 3.5. Limpiar Carrito

**Endpoint:** `DELETE /api/shoppingcart/clear`

**Descripción:** Elimina todos los productos del carrito.

**Autorización:** ? Requiere autenticación

**Response Success (200 OK):**
```json
{
  "message": "Cart cleared successfully"
}
```

---

### 3.6. Convertir Carrito en Factura

**Endpoint:** `POST /api/shoppingcart/checkout`

**Descripción:** Convierte el carrito en una factura.

**Autorización:** ? Requiere autenticación

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "clientId": 5,
  "observations": "Entrega programada para el 20 de enero"
}
```

**Parámetros Requeridos:**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| clientId | int | ID del cliente | Requerido, mayor a 0 |
| observations | string | Observaciones | Opcional, máx 500 caracteres |

**?? Proceso Automático:**
1. Valida que el carrito tenga productos
2. Valida stock de todos los productos
3. Crea la factura con los productos del carrito
4. Limpia el carrito
5. Genera número de factura automáticamente

**Response Success (201 Created):**
```json
{
  "success": true,
  "message": "Checkout completed successfully",
  "invoice": {
    "invoiceId": 30,
    "invoiceNumber": "FAC-2024-00030",
    "clientId": 5,
    "subtotal": 1825.48,
    "tax": 219.06,
    "total": 2044.54,
"status": "Draft",
    "invoiceDetails": [
      {
 "productId": 15,
        "productName": "Laptop Dell Inspiron",
        "quantity": 2,
    "unitPrice": 899.99,
        "subtotal": 1799.98
      },
      {
        "productId": 20,
   "productName": "Mouse Inalámbrico",
        "quantity": 1,
        "unitPrice": 25.50,
     "subtotal": 25.50
      }
    ]
  }
}
```

**Response Errors:**

**400 Bad Request - Carrito vacío:**
```json
{
  "message": "El carrito está vacío"
}
```

**400 Bad Request - Stock insuficiente:**
```json
{
  "message": "Stock insuficiente para 'Laptop Dell Inspiron'. Disponible: 1, Requerido: 2"
}
```

---

## ?? 4. Imágenes (Images)

### Base URL
```
/api/images
```

**Autorización General:** ? Requiere rol `Administrator`

---

### 4.1. Subir Imagen

**Endpoint:** `POST /api/images/upload`

**Descripción:** Sube una imagen a Cloudinary.

**Autorización:** ? Requiere rol `Administrator`

**Content-Type:** `multipart/form-data`

**Request Body (Form Data):**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| file | file | Archivo de imagen | Requerido, formatos: jpg, png, gif |
| folder | string | Carpeta en Cloudinary | Opcional, default: "general" |

**Carpetas Disponibles:**
- `products` - Imágenes de productos
- `profiles` - Fotos de perfil
- `general` - Imágenes generales

**Response Success (200 OK):**
```json
{
  "success": true,
  "message": "Image uploaded successfully",
  "secureUrl": "https://res.cloudinary.com/your-cloud/image/upload/v123456789/products/abc123.jpg",
  "publicId": "products/abc123",
  "width": 1920,
  "height": 1080,
  "format": "jpg",
  "resourceType": "image"
}
```

**Response Errors:**

**400 Bad Request - No se proporcionó archivo:**
```json
{
  "success": false,
  "message": "No file provided"
}
```

**400 Bad Request - Formato no soportado:**
```json
{
  "success": false,
  "message": "Invalid file format. Supported: jpg, png, gif"
}
```

---

### 4.2. Eliminar Imagen

**Endpoint:** `DELETE /api/images/delete/{publicId}`

**Descripción:** Elimina una imagen de Cloudinary por su Public ID.

**Autorización:** ? Requiere rol `Administrator`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| publicId | string | Public ID de Cloudinary (URL encoded) |

**Ejemplo Request:**
```
DELETE /api/images/delete/products%2Fabc123
```

**Response Success (200 OK):**
```json
{
  "success": true,
  "message": "Image deleted successfully"
}
```

**Response Errors:**
- **404 Not Found:** Imagen no encontrada

---

### 4.3. Eliminar Imagen por URL

**Endpoint:** `DELETE /api/images/delete-by-url`

**Descripción:** Elimina una imagen proporcionando su URL completa.

**Autorización:** ? Requiere rol `Administrator`

**Query Parameters:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| url | string | URL completa de la imagen |

**Ejemplo Request:**
```
DELETE /api/images/delete-by-url?url=https://res.cloudinary.com/.../products/abc123.jpg
```

**Response Success (200 OK):**
```json
{
  "success": true,
  "message": "Image deleted successfully"
}
```

---

## ?? 5. Roles

### Base URL
```
/api/roles
```

**Autorización General:** ? Requiere rol `Administrator`

---

### 5.1. Obtener Todos los Roles

**Endpoint:** `GET /api/roles`

**Descripción:** Lista todos los roles disponibles en el sistema.

**Response Success (200 OK):**
```json
[
  {
    "id": "role-guid-1",
    "name": "Administrator",
    "normalizedName": "ADMINISTRATOR",
    "description": "Acceso completo al sistema"
  },
  {
    "id": "role-guid-2",
    "name": "User",
    "normalizedName": "USER",
  "description": "Usuario estándar con permisos limitados"
  }
]
```

---

### 5.2. Crear Rol

**Endpoint:** `POST /api/roles`

**Descripción:** Crea un nuevo rol.

**Content-Type:** `application/json`

**Request Body:**
```json
{
  "name": "Manager",
  "description": "Gerente con permisos de supervisión"
}
```

---

### 5.3. Actualizar Rol

**Endpoint:** `PUT /api/roles/{id}`

**Descripción:** Actualiza un rol existente.

---

### 5.4. Eliminar Rol

**Endpoint:** `DELETE /api/roles/{id}`

**Descripción:** Elimina un rol del sistema.

---

## ?? 6. Códigos de Estado HTTP

### Códigos de Éxito (2xx)

| Código | Nombre | Descripción | Uso en API |
|--------|--------|-------------|------------|
| 200 | OK | Solicitud exitosa | GET, PUT, DELETE |
| 201 | Created | Recurso creado | POST (crear) |
| 204 | No Content | Éxito sin contenido | DELETE |

### Códigos de Error del Cliente (4xx)

| Código | Nombre | Descripción | Uso en API |
|--------|--------|-------------|------------|
| 400 | Bad Request | Datos inválidos o validación fallida | Validaciones |
| 401 | Unauthorized | No autenticado | Sin token o token inválido |
| 403 | Forbidden | Sin permisos | Rol insuficiente |
| 404 | Not Found | Recurso no encontrado | ID inexistente |
| 409 | Conflict | Conflicto (duplicado) | Email/código duplicado |

### Códigos de Error del Servidor (5xx)

| Código | Nombre | Descripción | Uso en API |
|--------|--------|-------------|------------|
| 500 | Internal Server Error | Error interno | Excepción no controlada |
| 503 | Service Unavailable | Servicio no disponible | BD o servicio externo caído |

---

## ?? 7. Modelo de Datos Completo

### Entidades Principales

```
???????????????????????????????????????????????????????????????
?    ApplicationUser     ?
???????????????????????????????????????????????????????????????
? + Id: string (PK)   ?
? + Identification: string   ?
? + UserName: string       ?
? + Email: string      ?
? + PasswordHash: string     ?
? + ProfileImageUri: string            ?
? + EmailConfirmed: boolean     ?
? + IsBlocked: boolean        ?
? + LockoutEnd: DateTimeOffset?  ?
? + CreatedAt: DateTime   ?
? + UpdatedAt: DateTime?     ?
???????????????????????????????????????????????????????????????
      ? 1:N
???????????????????????????????????????????????????????????????
?    Invoice       ?
???????????????????????????????????????????????????????????????
? + InvoiceId: int (PK)        ?
? + InvoiceNumber: string (Unique)   ?
? + ClientId: int (FK)     ?
? + UserId: string (FK)       ?
? + IssueDate: DateTime        ?
? + Subtotal: decimal         ?
? + Tax: decimal     ?
? + Total: decimal         ?
? + Status: InvoiceStatus (Draft/Finalized/Cancelled)?
? + Observations: string  ?
? + CancelReason: string?       ?
? + IsActive: boolean              ?
? + CreatedAt: DateTime        ?
? + UpdatedAt: DateTime?        ?
? + DeletedAt: DateTime?     ?
???????????????????????????????????????????????????????????????
    ? 1:N        ? 1:N
??????????????????????????????? ???????????????????????????????
?     InvoiceDetail  ? ?      Payment    ?
??????????????????????????????? ???????????????????????????????
? + InvoiceDetailId: int (PK) ? ? + PaymentId: int (PK)       ?
? + InvoiceId: int (FK)       ? ? + InvoiceId: int (FK)       ?
? + ProductId: int (FK)    ? ? + PaymentMethodId: string   ?
? + Quantity: int          ? ? + Amount: decimal    ?
? + UnitPrice: decimal      ? ? + TransactionId: string     ?
? + Subtotal: decimal         ? ? + Status: PaymentStatus     ?
??????????????????????????????? ? + PaymentDate: DateTime     ?
            ?      ? + ProcessedAt: DateTime?    ?
     ?          ???????????????????????????????
            ? N:1
???????????????????????????????????????????????????????????????
?            Product  ?
???????????????????????????????????????????????????????????????
? + ProductId: int (PK)          ?
? + Code: string (Unique) ?
? + Name: string          ?
? + Description: string              ?
? + Price: decimal              ?
? + Stock: int ?
? + ImageUri: string         ?
? + IsActive: boolean        ?
? + CreatedAt: DateTime             ?
? + UpdatedAt: DateTime?            ?
? + DeletedAt: DateTime?             ?
???????????????????????????????????????????????????????????????

???????????????????????????????????????????????????????????????
?     Client         ?
???????????????????????????????????????????????????????????????
? + ClientId: int (PK) ?
? + IdentificationType: string (Cedula/RUC/Pasaporte)       ?
? + IdentificationNumber: string (Unique)                 ?
? + FirstName: string   ?
? + LastName: string      ?
? + Phone: string?
? + Email: string     ?
? + Address: string        ?
? + IsActive: boolean             ?
? + CreatedAt: DateTime             ?
? + UpdatedAt: DateTime?               ?
? + DeletedAt: DateTime? ?
???????????????????????????????????????????????????????????????
```

---

## ?? 8. Ejemplos de Integración

### 8.1. Flujo Completo: Compra de Productos

**Paso 1: Login**
```bash
curl -X POST https://api.example.com/api/auth/login \
  -H "Content-Type: application/json" \
-d '{
    "email": "user@example.com",
    "password": "UserPass@123"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-12-31T23:59:59Z"
}
```

---

**Paso 2: Buscar Productos**
```bash
curl -X GET "https://api.example.com/api/product?pageNumber=1&pageSize=10&searchTerm=Laptop" \
  -H "Authorization: Bearer {token}"
```

---

**Paso 3: Agregar al Carrito**
```bash
curl -X POST https://api.example.com/api/shoppingcart/add-item \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 15,
 "quantity": 2
  }'
```

---

**Paso 4: Ver Carrito**
```bash
curl -X GET https://api.example.com/api/shoppingcart/my-cart \
  -H "Authorization: Bearer {token}"
```

---

**Paso 5: Crear Factura (Checkout)**
```bash
curl -X POST https://api.example.com/api/shoppingcart/checkout \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": 5,
    "observations": "Entrega urgente"
  }'
```

**Response:**
```json
{
  "success": true,
  "invoice": {
    "invoiceId": 30,
    "invoiceNumber": "FAC-2024-00030",
    "total": 2044.54
  }
}
```

---

**Paso 6: Finalizar Factura**
```bash
curl -X POST https://api.example.com/api/invoice/30/finalize \
  -H "Authorization: Bearer {token}"
```

---

**Paso 7: Procesar Pago**
```bash
curl -X POST https://api.example.com/api/payment/process \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "invoiceId": 30,
    "paymentMethodId": "credit_card",
    "amount": 2044.54,
    "additionalInfo": "Visa ****1234"
  }'
```

**Response:**
```json
{
  "success": true,
  "transactionId": "TXN-2024-ABC123",
  "status": "Completed",
  "message": "Payment processed successfully"
}
```

---

### 8.2. Headers Estándar

**Para todas las peticiones autenticadas:**
```
Authorization: Bearer {jwt-token}
Content-Type: application/json
Accept: application/json
```

**Para peticiones con archivos:**
```
Authorization: Bearer {jwt-token}
Content-Type: multipart/form-data
```

---

### 8.3. Manejo de Errores Estándar

**Estructura de Error:**
```json
{
  "message": "Descripción del error",
  "errorCode": "ERROR_CODE",
  "errors": {
    "Campo1": ["Error 1", "Error 2"],
    "Campo2": ["Error 3"]
  }
}
```

---

## ?? Resumen de Endpoints por Rol

### Administrator (Acceso Completo)
? Todos los endpoints

### User (Acceso Limitado)
? GET - Lectura de productos, clientes, facturas  
? POST - Crear facturas, procesar pagos  
? Gestión de carrito propio  
? Ver su propio perfil  
? Crear/Editar/Eliminar usuarios  
? Crear/Editar productos y clientes  
? Eliminar facturas  
? Procesar reembolsos

### Sin Autenticación
? POST /api/auth/login  
? Todos los demás endpoints

---

**FIN DE LA DOCUMENTACIÓN**

---

**Fecha de última actualización:** 2024-01-15  
**Versión del proyecto:** .NET 9  
**Versión de la documentación:** 1.0

**Contacto:**  
Para soporte o consultas sobre la API, contactar al equipo de desarrollo.
