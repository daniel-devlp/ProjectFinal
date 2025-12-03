# API Documentation - Parte 3: Gestión de Productos

## ?? Índice
- [Operaciones CRUD de Productos](#operaciones-crud-de-productos)
- [Búsqueda y Filtrado](#búsqueda-y-filtrado)
- [Gestión de Stock](#gestión-de-stock)
- [Gestión de Borrado Lógico](#gestión-de-borrado-lógico)
- [Estadísticas](#estadísticas)

---

## ?? Operaciones CRUD de Productos

### GET `/api/Product/{id}`
Obtiene un producto específico por ID (solo productos activos).

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer): ID del producto

**Respuestas:**

**? 200 OK**
```json
{
  "productId": 1,
  "code": "PROD-001",
  "name": "Laptop Dell XPS 15",
  "description": "Laptop de alto rendimiento con procesador Intel i7",
  "price": 1299.99,
  "stock": 25,
  "isActive": true,
  "imageUri": "https://res.cloudinary.com/.../product-image.jpg",
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-06-15T14:30:00Z",
  "deletedAt": null
}
```

**? 404 Not Found**
```json
{
  "message": "Product not found"
}
```

---

### GET `/api/Product`
Obtiene todos los productos activos con paginación y búsqueda.

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
```

**Query Parameters:**
- `pageNumber` (integer, opcional): Número de página (default: 1)
- `pageSize` (integer, opcional): Tamaño de página (default: 10)
- `searchTerm` (string, opcional): Término de búsqueda (busca en código, nombre, descripción)

**Ejemplo:**
```
GET /api/Product?pageNumber=1&pageSize=20&searchTerm=laptop
```

**Respuestas:**

**? 200 OK**
```json
{
  "items": [
    {
    "productId": 1,
    "code": "PROD-001",
      "name": "Laptop Dell XPS 15",
      "description": "Laptop de alto rendimiento",
      "price": 1299.99,
      "stock": 25,
      "isActive": true,
   "imageUri": "https://res.cloudinary.com/.../image.jpg",
"createdAt": "2024-01-01T10:00:00Z"
    }
  ],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 15,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

---

### POST `/api/Product`
Crea un nuevo producto con imagen opcional.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Request Body (Form-Data):**
```
code: string (requerido) - Código único del producto
name: string (requerido) - Nombre del producto
description: string (opcional) - Descripción del producto
price: decimal (requerido) - Precio unitario
stock: integer (requerido) - Cantidad en inventario
image: file (opcional) - Imagen del producto
```

**Validaciones del Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `code` | string | ? | Formato especial (ej: PROD-001), único, se normaliza a mayúsculas |
| `name` | string | ? | 2-200 caracteres |
| `description` | string | ? | Máx 1000 caracteres |
| `price` | decimal | ? | Debe ser positivo (> 0) |
| `stock` | integer | ? | Debe ser no negativo (>= 0) |
| `image` | file | ? | Archivo de imagen, se sube a Cloudinary |

**Validaciones Especiales:**

**Código del Producto** (`code`):
- Formato validado por `[ProductCodeFormat]`
- Debe ser único en el sistema
- Se normaliza automáticamente a mayúsculas
- Ejemplos válidos: `"PROD-001"`, `"LAP-2024-01"`

**Precio** (`price`):
- Validado por `[PositiveDecimal]`
- Debe ser mayor a 0
- Acepta decimales (ej: 1299.99)

**Stock** (`stock`):
- Validado por `[NonNegativeInteger]`
- Debe ser mayor o igual a 0
- Solo números enteros

**Imagen** (`image`):
- Archivo opcional
- Se sube a Cloudinary en la carpeta "products"
- Si falla la creación del producto, la imagen se elimina automáticamente
- Formatos aceptados: JPG, PNG, etc.

**Respuestas:**

**? 201 Created**
```json
{
  "success": true,
  "message": "Product created successfully with image",
  "product": {
    "code": "PROD-001",
    "name": "Laptop Dell XPS 15",
 "description": "Laptop de alto rendimiento",
    "price": 1299.99,
    "stock": 25,
    "imageUrl": "https://res.cloudinary.com/.../product-001.jpg"
  }
}
```

**Sin imagen:**
```json
{
  "success": true,
  "message": "Product created successfully",
  "product": {
    "code": "PROD-002",
    "name": "Mouse Logitech",
    "description": "Mouse inalámbrico",
    "price": 29.99,
    "stock": 100,
    "imageUrl": null
  }
}
```

**? 400 Bad Request** - Validación fallida
```json
{
  "message": "Validation failed",
  "errors": {
    "Code": ["El código del producto ya existe"],
    "Price": ["El precio debe ser un número positivo"],
    "Stock": ["El stock no puede ser negativo"]
  }
}
```

**? 400 Bad Request** - Error al subir imagen
```json
{
  "success": false,
  "message": "Failed to upload image",
  "error": "Invalid image format"
}
```

**? 400 Bad Request** - Código duplicado
```json
{
  "message": "Ya existe un producto con este código"
}
```

---

### PUT `/api/Product/{id}`
Actualiza un producto existente, incluyendo imagen opcional.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Path Parameters:**
- `id` (integer): ID del producto

**Request Body (Form-Data):**
```
code: string (requerido) - Código del producto
name: string (requerido) - Nombre del producto
description: string (opcional) - Descripción
price: decimal (requerido) - Precio
stock: integer (requerido) - Stock
isActive: boolean (requerido) - Estado activo/inactivo
image: file (opcional) - Nueva imagen del producto
```

**Validaciones:**
- Todas las validaciones de creación aplican
- Si se proporciona nueva imagen, se elimina la anterior automáticamente
- El código debe ser único (excepto el del producto actual)

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Product updated successfully with new image",
  "product": {
    "id": 1,
    "code": "PROD-001",
    "name": "Laptop Dell XPS 15 (Actualizado)",
    "imageUrl": "https://res.cloudinary.com/.../new-image.jpg"
  }
}
```

**Sin cambio de imagen:**
```json
{
  "message": "Product updated successfully",
  "product": {
    "id": 1,
"code": "PROD-001",
    "name": "Laptop Dell XPS 15 (Actualizado)",
    "imageUrl": "https://res.cloudinary.com/.../original-image.jpg"
  }
}
```

**? 404 Not Found**
```json
{
  "message": "Product not found"
}
```

---

### DELETE `/api/Product/{id}`
Realiza borrado lógico de un producto (cambia `IsActive` a `false`).

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer): ID del producto

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Product deleted successfully (soft delete)"
}
```

**? 404 Not Found**
```json
{
  "message": "Producto no encontrado"
}
```

**? 400 Bad Request**
```json
{
  "message": "El producto ya está eliminado"
}
```

---

## ?? Búsqueda y Filtrado

### GET `/api/Product/by-code/{code}`
Busca un producto por código.

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `code` (string): Código del producto

**Ejemplo:**
```
GET /api/Product/by-code/PROD-001
```

**Respuestas:**

**? 200 OK**
```json
{
  "productId": 1,
  "code": "PROD-001",
  "name": "Laptop Dell XPS 15",
  "description": "Laptop de alto rendimiento",
  "price": 1299.99,
  "stock": 25,
  "isActive": true,
  "imageUri": "https://res.cloudinary.com/.../image.jpg"
}
```

**? 404 Not Found**
```json
{
  "message": "Product not found"
}
```

---

### GET `/api/Product/low-stock`
Obtiene productos con stock bajo.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Query Parameters:**
- `threshold` (integer, opcional): Umbral de stock bajo (default: 10)

**Ejemplo:**
```
GET /api/Product/low-stock?threshold=15
```

**Respuestas:**

**? 200 OK**
```json
[
  {
    "productId": 5,
    "code": "PROD-005",
    "name": "Mouse Logitech",
    "stock": 8,
    "price": 29.99,
    "isActive": true
  },
  {
    "productId": 12,
    "code": "PROD-012",
    "name": "Teclado Mecánico",
    "stock": 3,
    "price": 79.99,
    "isActive": true
  }
]
```

---

## ?? Gestión de Stock

### PATCH `/api/Product/{id}/stock`
Actualiza solo el stock de un producto.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Path Parameters:**
- `id` (integer): ID del producto

**Request Body:**
```json
{
  "productId": 1,
  "quantity": 50,
  "reason": "Reposición de inventario"
}
```

**Validaciones del Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `productId` | integer | ? | Debe coincidir con ID de la URL |
| `quantity` | integer | ? | Debe ser mayor a 0 |
| `reason` | string | ? | Descripción del cambio |

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Stock updated successfully"
}
```

**? 400 Bad Request** - ID mismatch
```json
{
  "message": "ID mismatch"
}
```

**? 400 Bad Request** - Cantidad inválida
```json
{
  "message": "La cantidad debe ser mayor a cero"
}
```

**? 404 Not Found**
```json
{
  "message": "Producto no encontrado"
}
```

---

## ??? Gestión de Borrado Lógico

### GET `/api/Product/all-including-deleted`
Obtiene todos los productos incluyendo eliminados.

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
GET /api/Product/all-including-deleted?pageNumber=1&pageSize=20
```

**Respuestas:**

**? 200 OK**
```json
{
  "items": [
 {
      "productId": 1,
      "code": "PROD-001",
      "name": "Laptop Dell XPS 15",
      "price": 1299.99,
 "stock": 25,
      "isActive": true,
      "deletedAt": null
    },
    {
      "productId": 2,
      "code": "PROD-002",
      "name": "Monitor Samsung 27\"",
  "price": 349.99,
      "stock": 0,
      "isActive": false,
      "deletedAt": "2024-05-20T10:00:00Z"
    }
  ],
  "totalCount": 160,
  "pageNumber": 1,
  "pageSize": 20
}
```

---

### GET `/api/Product/deleted`
Obtiene solo los productos eliminados.

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
    "productId": 2,
    "code": "PROD-002",
    "name": "Monitor Samsung 27\"",
    "description": "Monitor Full HD",
    "price": 349.99,
    "stock": 0,
    "isActive": false,
    "imageUri": "https://res.cloudinary.com/.../monitor.jpg",
    "deletedAt": "2024-05-20T10:00:00Z"
  }
]
```

---

### POST `/api/Product/{id}/restore`
Restaura un producto eliminado lógicamente.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer): ID del producto

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Product restored successfully"
}
```

**? 404 Not Found**
```json
{
  "message": "Producto no encontrado"
}
```

**? 400 Bad Request**
```json
{
  "message": "El producto no está eliminado"
}
```

---

## ?? Estadísticas

### GET `/api/Product/statistics`
Obtiene estadísticas generales de productos.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Respuestas:**

**? 200 OK**
```json
{
  "activeProducts": 150,
  "deletedProducts": 10,
  "totalProducts": 160,
  "deletionRate": 6.25
}
```

---

## ?? Notas Importantes

### Manejo de Imágenes
- **Subida**: Las imágenes se suben a Cloudinary en la carpeta "products"
- **Actualización**: Al actualizar un producto con nueva imagen, se elimina la anterior
- **Eliminación**: Al eliminar un producto (borrado lógico), la imagen permanece
- **Rollback**: Si falla la creación del producto, la imagen se elimina automáticamente

### Normalización de Datos
- **Código**: Se normaliza automáticamente a mayúsculas
  - Input: `"prod-001"` ? Output: `"PROD-001"`

### Validaciones Personalizadas
- **[ProductCodeFormat]**: Valida formato del código de producto
- **[PositiveDecimal]**: Valida que el precio sea positivo
- **[NonNegativeInteger]**: Valida que el stock no sea negativo

### Búsqueda (searchTerm)
El parámetro `searchTerm` busca en los siguientes campos:
- Código (`code`)
- Nombre (`name`)
- Descripción (`description`)

### Borrado Lógico
- Los productos eliminados **NO** aparecen en consultas estándar
- Solo administradores pueden ver productos eliminados
- Los productos eliminados mantienen su información e imagen
- Se registra la fecha de eliminación en `deletedAt`
- Los productos pueden ser restaurados

### Stock Bajo
- Por defecto, se considera "stock bajo" cuando hay 10 o menos unidades
- El umbral es configurable mediante el parámetro `threshold`
- Útil para alertas de reposición de inventario

### Content-Type
- **Creación y Actualización**: Usa `multipart/form-data` porque acepta archivos
- **Actualización de Stock**: Usa `application/json` porque solo maneja datos

### Ejemplos de Códigos de Producto Válidos
```
PROD-001
LAP-2024-01
MOU-WIRELESS-099
KEYB-MECH-001
```

### Códigos de Error Comunes
- **400**: Validación fallida, datos duplicados o error de imagen
- **401**: No autenticado
- **403**: No autorizado (falta rol Administrator)
- **404**: Producto no encontrado
- **500**: Error interno del servidor

### Ejemplo Completo: Crear Producto con Imagen

**Request (Form-Data):**
```
POST /api/Product
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: multipart/form-data

code=PROD-025
name=Auriculares Sony WH-1000XM5
description=Auriculares con cancelación de ruido
price=399.99
stock=15
image=[binary file data]
```

**Response:**
```json
{
  "success": true,
  "message": "Product created successfully with image",
  "product": {
    "code": "PROD-025",
    "name": "Auriculares Sony WH-1000XM5",
    "description": "Auriculares con cancelación de ruido",
    "price": 399.99,
"stock": 15,
    "imageUrl": "https://res.cloudinary.com/demo/image/upload/v1234567890/products/prod-025.jpg"
  }
}
```
