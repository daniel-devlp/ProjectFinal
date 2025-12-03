# API Documentation - Parte 2: Gestión de Clientes

## ?? Índice
- [Operaciones CRUD de Clientes](#operaciones-crud-de-clientes)
- [Búsqueda y Filtrado](#búsqueda-y-filtrado)
- [Gestión de Borrado Lógico](#gestión-de-borrado-lógico)
- [Estadísticas](#estadísticas)

---

## ?? Operaciones CRUD de Clientes

### GET `/api/Client/{id}`
Obtiene un cliente específico por ID (solo clientes activos).

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer): ID del cliente

**Respuestas:**

**? 200 OK**
```json
{
  "clientId": 1,
  "identificationType": "CEDULA",
  "identificationNumber": "1234567890",
  "firstName": "Juan",
  "lastName": "Pérez",
  "phone": "0991234567",
  "email": "juan.perez@example.com",
  "address": "Av. Principal 123, Quito",
  "isActive": true,
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-06-15T14:30:00Z",
  "deletedAt": null
}
```

**? 404 Not Found**
```json
{
  "message": "Client not found"
}
```

**? 400 Bad Request**
```json
{
  "message": "El ID debe ser mayor a cero"
}
```

---

### GET `/api/Client`
Obtiene todos los clientes activos con paginación y búsqueda.

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
```

**Query Parameters:**
- `pageNumber` (integer, opcional): Número de página (default: 1)
- `pageSize` (integer, opcional): Tamaño de página (default: 10)
- `searchTerm` (string, opcional): Término de búsqueda (busca en nombre, apellido, email, identificación)

**Ejemplo de URL:**
```
GET /api/Client?pageNumber=1&pageSize=20&searchTerm=juan
```

**Respuestas:**

**? 200 OK**
```json
{
  "items": [
    {
      "clientId": 1,
      "identificationType": "CEDULA",
      "identificationNumber": "1234567890",
  "firstName": "Juan",
      "lastName": "Pérez",
      "phone": "0991234567",
      "email": "juan.perez@example.com",
      "address": "Av. Principal 123, Quito",
      "isActive": true,
   "createdAt": "2024-01-01T10:00:00Z",
   "updatedAt": null,
      "deletedAt": null
    }
  ],
  "totalCount": 45,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "firstItemIndex": 1,
  "lastItemIndex": 10,
  "isEmpty": false
}
```

**? 400 Bad Request**
```json
{
  "message": "El número de página debe ser mayor a 0"
}
```

---

### POST `/api/Client`
Crea un nuevo cliente con validaciones completas.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Request Body:**
```json
{
  "identificationType": "CEDULA",
  "identificationNumber": "1234567890",
  "firstName": "Juan",
  "lastName": "Pérez",
  "phone": "0991234567",
  "email": "juan.perez@example.com",
  "address": "Av. Principal 123, Quito"
}
```

**Validaciones del Request Body:**

| Campo | Tipo | Requerido | Validación |
|-------|------|-----------|------------|
| `identificationType` | string | ? | Debe ser "CEDULA", "PASAPORTE", "RUC" |
| `identificationNumber` | string | ? | Formato según tipo (10 dígitos para cédula, 13 para RUC) |
| `firstName` | string | ? | 2-50 caracteres, solo letras y espacios |
| `lastName` | string | ? | 2-50 caracteres, solo letras y espacios |
| `phone` | string | ? | Formato teléfono ecuatoriano (09XXXXXXXX o 02XXXXXXX) |
| `email` | string | ? | Email válido, máx 100 caracteres, único |
| `address` | string | ? | Máx 200 caracteres |

**Validaciones Especiales:**

**Tipo de Identificación** (`identificationType`):
- Valores permitidos: `"CEDULA"`, `"PASAPORTE"`, `"RUC"`
- Validado por `[ClientIdentificationTypeValidation]`

**Número de Identificación** (`identificationNumber`):
- **CEDULA**: 10 dígitos numéricos, validación de dígito verificador
- **RUC**: 13 dígitos numéricos
- **PASAPORTE**: Formato alfanumérico
- Único en el sistema
- Validado por `[ClientIdentificationValidation]`

**Teléfono** (`phone`):
- Celular: `09XXXXXXXX` (10 dígitos, inicia con 09)
- Convencional: `02XXXXXXX` (9 dígitos, código de área)
- Validado por `[EcuadorianPhone]`

**Nombre y Apellido**:
- Solo letras (incluyendo á, é, í, ó, ú, ñ) y espacios
- Expresión regular: `^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$`

**Respuestas:**

**? 201 Created**
```json
{
  "message": "Client created successfully"
}
```

**? 400 Bad Request** - Validación fallida
```json
{
  "message": "Validation failed",
  "errors": {
    "IdentificationNumber": ["El número de identificación ya existe"],
    "Phone": ["El teléfono debe ser un número ecuatoriano válido"],
    "Email": ["Formato de email inválido"]
  }
}
```

**? 400 Bad Request** - Identificación duplicada
```json
{
  "message": "Ya existe un cliente con esta identificación"
}
```

**? 400 Bad Request** - Email duplicado
```json
{
  "message": "Ya existe un cliente con este email"
}
```

---

### PUT `/api/Client/{id}`
Actualiza un cliente existente.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
Content-Type: application/json
```

**Path Parameters:**
- `id` (integer): ID del cliente

**Request Body:**
```json
{
  "clientId": 1,
  "identificationType": "CEDULA",
  "identificationNumber": "1234567890",
  "firstName": "Juan Carlos",
  "lastName": "Pérez García",
  "phone": "0991234567",
  "email": "juan.perez@example.com",
  "address": "Av. Principal 456, Quito"
}
```

**Validaciones:**
- `clientId` en el body debe coincidir con `id` de la URL
- Todas las validaciones de creación aplican
- El número de identificación debe ser único (excepto el del cliente actual)
- El email debe ser único (excepto el del cliente actual)

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Client updated successfully"
}
```

**? 400 Bad Request** - ID mismatch
```json
{
  "message": "ID mismatch",
  "details": "URL ID (5) does not match body ID (3)"
}
```

**? 404 Not Found**
```json
{
  "message": "Cliente no encontrado"
}
```

---

### DELETE `/api/Client/{id}`
Realiza borrado lógico de un cliente (cambia `IsActive` a `false`).

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer): ID del cliente

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Client deleted successfully (soft delete)"
}
```

**? 404 Not Found**
```json
{
  "message": "Cliente no encontrado"
}
```

**? 400 Bad Request**
```json
{
  "message": "El cliente ya está eliminado"
}
```

---

## ?? Búsqueda y Filtrado

### GET `/api/Client/by-identification/{identification}`
Busca un cliente por número de identificación.

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `identification` (string): Número de identificación (cédula, RUC o pasaporte)

**Ejemplo:**
```
GET /api/Client/by-identification/1234567890
```

**Respuestas:**

**? 200 OK**
```json
{
"clientId": 1,
  "identificationType": "CEDULA",
  "identificationNumber": "1234567890",
  "firstName": "Juan",
  "lastName": "Pérez",
  "phone": "0991234567",
  "email": "juan.perez@example.com",
  "address": "Av. Principal 123, Quito",
  "isActive": true
}
```

**? 404 Not Found**
```json
{
  "message": "Client not found"
}
```

---

## ??? Gestión de Borrado Lógico

### GET `/api/Client/all-including-deleted`
Obtiene todos los clientes incluyendo eliminados (solo administradores).

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
GET /api/Client/all-including-deleted?pageNumber=1&pageSize=20
```

**Respuestas:**

**? 200 OK**
```json
{
  "items": [
  {
      "clientId": 1,
      "identificationType": "CEDULA",
 "identificationNumber": "1234567890",
      "firstName": "Juan",
      "lastName": "Pérez",
      "isActive": true,
      "deletedAt": null
    },
    {
      "clientId": 2,
      "identificationType": "CEDULA",
      "identificationNumber": "0987654321",
      "firstName": "María",
      "lastName": "González",
      "isActive": false,
      "deletedAt": "2024-05-20T10:00:00Z"
    }
  ],
  "totalCount": 50,
  "pageNumber": 1,
  "pageSize": 20
}
```

---

### GET `/api/Client/deleted`
Obtiene solo los clientes eliminados.

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
    "clientId": 2,
    "identificationType": "CEDULA",
    "identificationNumber": "0987654321",
    "firstName": "María",
    "lastName": "González",
    "phone": "0987654321",
    "email": "maria@example.com",
    "address": "Calle Secundaria 789",
    "isActive": false,
    "deletedAt": "2024-05-20T10:00:00Z"
  }
]
```

---

### POST `/api/Client/{id}/restore`
Restaura un cliente eliminado lógicamente.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (integer): ID del cliente

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Client restored successfully"
}
```

**? 404 Not Found**
```json
{
  "message": "Cliente no encontrado"
}
```

**? 400 Bad Request**
```json
{
  "message": "El cliente no está eliminado"
}
```

---

## ?? Estadísticas

### GET `/api/Client/statistics`
Obtiene estadísticas generales de clientes.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Respuestas:**

**? 200 OK**
```json
{
  "activeClients": 45,
  "deletedClients": 5,
  "totalClients": 50,
  "deletionRate": 10.0
}
```

---

## ?? Notas Importantes

### Paginación
- **PagedResult** es el modelo estándar para respuestas paginadas
- Propiedades calculadas automáticamente:
  - `totalPages`: Total de páginas
  - `hasNextPage`: Indica si hay página siguiente
  - `hasPreviousPage`: Indica si hay página anterior
  - `firstItemIndex`: Índice del primer elemento en la página actual
  - `lastItemIndex`: Índice del último elemento en la página actual
  - `isEmpty`: Indica si no hay elementos

### Borrado Lógico
- Los clientes eliminados **NO** aparecen en consultas estándar
- Solo administradores pueden ver clientes eliminados
- Los clientes eliminados mantienen su información completa
- Se registra la fecha de eliminación en `deletedAt`
- Los clientes pueden ser restaurados

### Validaciones Personalizadas
- **[ClientIdentificationTypeValidation]**: Valida tipo de identificación
- **[ClientIdentificationValidation]**: Valida formato de identificación según tipo
- **[EcuadorianPhone]**: Valida formato de teléfono ecuatoriano

### Búsqueda (searchTerm)
El parámetro `searchTerm` busca en los siguientes campos:
- Nombre (`firstName`)
- Apellido (`lastName`)
- Email (`email`)
- Número de identificación (`identificationNumber`)

### Códigos de Error Comunes
- **400**: Validación fallida o datos duplicados
- **401**: No autenticado
- **403**: No autorizado (falta rol Administrator)
- **404**: Cliente no encontrado
- **500**: Error interno del servidor

### Ejemplos de Validación de Cédula Ecuatoriana

**Cédula Válida:**
```json
{
  "identificationType": "CEDULA",
  "identificationNumber": "1714587896"
}
```

**Cédula Inválida (formato):**
```json
{
  "identificationType": "CEDULA",
  "identificationNumber": "123"
}
```
Error: "El número de cédula debe tener 10 dígitos"

**Cédula Inválida (dígito verificador):**
```json
{
  "identificationType": "CEDULA",
  "identificationNumber": "1714587899"
}
```
Error: "El número de cédula es inválido"

### Ejemplos de Validación de Teléfono

**Celular Válido:**
```
"phone": "0991234567"
```

**Convencional Válido:**
```
"phone": "022345678"
```

**Teléfono Inválido:**
```
"phone": "1234567"
```
Error: "El teléfono debe ser un número ecuatoriano válido"
