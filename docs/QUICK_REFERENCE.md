# ?? Guía Rápida de Referencia - API Endpoints

## ?? Autenticación

| Método | Endpoint | Descripción | Autorización |
|--------|----------|-------------|--------------|
| POST | `/api/Auth/login` | Iniciar sesión | Ninguna |

---

## ?? Usuarios

| Método | Endpoint | Descripción | Autorización |
|--------|----------|-------------|--------------|
| GET | `/api/Users` | Listar todos los usuarios | Administrator |
| GET | `/api/Users/{id}` | Obtener usuario por ID | Administrator |
| GET | `/api/Users/me` | Obtener perfil actual | Administrator, user |
| POST | `/api/Users` | Crear nuevo usuario | Administrator |
| PUT | `/api/Users/{id}` | Actualizar usuario | Administrator, user* |
| DELETE | `/api/Users/{id}` | Eliminar usuario | Administrator |
| POST | `/api/Users/{id}/unlock` | Desbloquear usuario | Administrator |
| DELETE | `/api/Users/{id}/profile-image` | Eliminar foto de perfil | Administrator, user* |

*user solo puede modificar su propio perfil

---

## ?? Clientes

| Método | Endpoint | Descripción | Autorización |
|--------|----------|-------------|--------------|
| GET | `/api/Client` | Listar clientes activos (paginado) | Administrator, user |
| GET | `/api/Client/{id}` | Obtener cliente por ID | Administrator, user |
| GET | `/api/Client/by-identification/{id}` | Buscar por identificación | Administrator, user |
| GET | `/api/Client/all-including-deleted` | Listar todos (incluye eliminados) | Administrator |
| GET | `/api/Client/deleted` | Listar solo eliminados | Administrator |
| GET | `/api/Client/statistics` | Obtener estadísticas | Administrator |
| POST | `/api/Client` | Crear nuevo cliente | Administrator |
| PUT | `/api/Client/{id}` | Actualizar cliente | Administrator |
| DELETE | `/api/Client/{id}` | Eliminar cliente (soft delete) | Administrator |
| POST | `/api/Client/{id}/restore` | Restaurar cliente eliminado | Administrator |

### Parámetros de Consulta (Query Params)
- `pageNumber`: Número de página (default: 1)
- `pageSize`: Tamaño de página (default: 10)
- `searchTerm`: Término de búsqueda

---

## ?? Productos

| Método | Endpoint | Descripción | Autorización |
|--------|----------|-------------|--------------|
| GET | `/api/Product` | Listar productos activos (paginado) | Administrator, user |
| GET | `/api/Product/{id}` | Obtener producto por ID | Administrator, user |
| GET | `/api/Product/by-code/{code}` | Buscar por código | Administrator, user |
| GET | `/api/Product/low-stock` | Productos con stock bajo | Administrator |
| GET | `/api/Product/all-including-deleted` | Listar todos (incluye eliminados) | Administrator |
| GET | `/api/Product/deleted` | Listar solo eliminados | Administrator |
| GET | `/api/Product/statistics` | Obtener estadísticas | Administrator |
| POST | `/api/Product` | Crear nuevo producto | Administrator |
| PUT | `/api/Product/{id}` | Actualizar producto | Administrator |
| PATCH | `/api/Product/{id}/stock` | Actualizar solo stock | Administrator |
| DELETE | `/api/Product/{id}` | Eliminar producto (soft delete) | Administrator |
| POST | `/api/Product/{id}/restore` | Restaurar producto eliminado | Administrator |

### Parámetros de Consulta (Query Params)
- `pageNumber`: Número de página (default: 1)
- `pageSize`: Tamaño de página (default: 10)
- `searchTerm`: Término de búsqueda
- `threshold`: Umbral de stock bajo (default: 10)

---

## ?? Facturas

| Método | Endpoint | Descripción | Autorización |
|--------|----------|-------------|--------------|
| GET | `/api/Invoice` | Listar facturas activas (paginado) | Administrator, user |
| GET | `/api/Invoice/{id}` | Obtener factura por ID | Administrator, user |
| GET | `/api/Invoice/{id}/details` | Obtener detalles completos | Administrator, user |
| GET | `/api/Invoice/all-including-deleted` | Listar todas (incluye eliminadas) | Administrator |
| GET | `/api/Invoice/deleted` | Listar solo eliminadas | Administrator |
| GET | `/api/Invoice/statistics` | Obtener estadísticas | Administrator |
| POST | `/api/Invoice` | Crear nueva factura | Administrator, user |
| POST | `/api/Invoice/search` | Búsqueda avanzada | Administrator, user |
| PUT | `/api/Invoice/{id}` | Actualizar factura (solo Borrador) | Administrator |
| POST | `/api/Invoice/{id}/finalize` | Finalizar factura | Administrator, user |
| POST | `/api/Invoice/{id}/cancel` | Cancelar factura | Administrator |
| DELETE | `/api/Invoice/{id}` | Eliminar factura (soft delete) | Administrator |
| POST | `/api/Invoice/{id}/restore` | Restaurar factura eliminada | Administrator |

### Parámetros de Consulta (Query Params)
- `pageNumber`: Número de página (default: 1)
- `pageSize`: Tamaño de página (default: 10)
- `searchTerm`: Término de búsqueda

---

## ?? Modelos de Request/Response

### ClientCreateDto
```json
{
  "identificationType": "CEDULA|PASAPORTE|RUC",
  "identificationNumber": "string",
  "firstName": "string",
  "lastName": "string",
  "phone": "string",
  "email": "string",
  "address": "string"
}
```

### ProductCreateDto (Form-Data)
```
code: string
name: string
description: string (opcional)
price: decimal
stock: integer
image: file (opcional)
```

### InvoiceCreateDto
```json
{
  "clientId": integer,
  "observations": "string (opcional)",
  "invoiceDetails": [
    {
      "productId": integer,
      "quantity": integer,
      "unitPrice": decimal
    }
  ]
}
```

### UserCreateDto (Form-Data)
```
identificationNumber: string
email: string
userName: string
password: string
roles: string[] (opcional)
profileImage: file (opcional)
```

### InvoiceSearchDto
```json
{
  "pageNumber": integer,
  "pageSize": integer,
  "searchTerm": "string (opcional)",
  "fromDate": "datetime (opcional)",
  "toDate": "datetime (opcional)",
  "clientId": integer (opcional),
  "status": "string (opcional)",
  "includeDeleted": boolean
}
```

---

## ?? Códigos de Estado HTTP

| Código | Significado | Uso |
|--------|-------------|-----|
| 200 | OK | Operación exitosa |
| 201 | Created | Recurso creado exitosamente |
| 400 | Bad Request | Error de validación |
| 401 | Unauthorized | No autenticado |
| 403 | Forbidden | Sin permisos |
| 404 | Not Found | Recurso no encontrado |
| 500 | Internal Server Error | Error del servidor |

---

## ?? Autenticación

Todos los endpoints (excepto login) requieren:
```
Authorization: Bearer {token}
```

---

## ?? Content-Types

- **JSON**: `application/json`
- **Form-Data** (con archivos): `multipart/form-data`

---

## ?? Características Comunes

### Paginación
```
?pageNumber=1&pageSize=10&searchTerm=busqueda
```

### Borrado Lógico
- `DELETE /{resource}/{id}` - Marca como eliminado
- `POST /{resource}/{id}/restore` - Restaura
- `GET /{resource}/deleted` - Lista eliminados
- `GET /{resource}/all-including-deleted` - Lista todos

### Respuesta Paginada (PagedResult)
```json
{
  "items": [],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "firstItemIndex": 1,
  "lastItemIndex": 10,
  "isEmpty": false
}
```

---

## ?? Estados de Factura

| Estado | Descripción | Puede Modificarse | Afecta Stock |
|--------|-------------|-------------------|--------------|
| Borrador | Recién creada | ? Sí | ? No |
| Finalizada | Confirmada | ? No | ? Sí (reduce) |
| Cancelada | Anulada | ? No | ? Sí (restaura) |

---

## ?? Validaciones Ecuatorianas

### Cédula
- 10 dígitos numéricos
- Con validación de dígito verificador
- Ejemplo: `1714587896`

### RUC
- 13 dígitos numéricos
- Ejemplo: `1234567890001`

### Teléfono
- Celular: `09XXXXXXXX` (10 dígitos)
- Convencional: `02XXXXXXX` (9 dígitos)
- Ejemplos: `0991234567`, `022345678`

---

## ?? Ejemplos Rápidos

### Login
```bash
curl -X POST http://localhost:5000/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"Admin123!"}'
```

### Crear Cliente
```bash
curl -X POST http://localhost:5000/api/Client \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "identificationType":"CEDULA",
    "identificationNumber":"1714587896",
    "firstName":"Juan",
    "lastName":"Pérez",
    "phone":"0991234567",
    "email":"juan@example.com",
    "address":"Quito, Ecuador"
  }'
```

### Listar Productos con Paginación
```bash
curl -X GET "http://localhost:5000/api/Product?pageNumber=1&pageSize=20&searchTerm=laptop" \
  -H "Authorization: Bearer {token}"
```

### Crear Factura
```bash
curl -X POST http://localhost:5000/api/Invoice \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "clientId":5,
    "observations":"Venta especial",
    "invoiceDetails":[
      {"productId":10,"quantity":2,"unitPrice":750.00}
    ]
  }'
```

### Finalizar Factura
```bash
curl -X POST http://localhost:5000/api/Invoice/125/finalize \
  -H "Authorization: Bearer {token}"
```

---

## ?? Documentación Completa

Para más detalles, consulta:
1. [Autenticación y Usuarios](./API_DOCUMENTATION_PART_1_AUTENTICACION.md)
2. [Gestión de Clientes](./API_DOCUMENTATION_PART_2_CLIENTES.md)
3. [Gestión de Productos](./API_DOCUMENTATION_PART_3_PRODUCTOS.md)
4. [Gestión de Facturas](./API_DOCUMENTATION_PART_4_FACTURAS.md)

---

**ProjectFinal API - Guía Rápida v1.0**
