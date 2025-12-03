# ?? DOCUMENTACIÓN DEL PROYECTO - PARTE 2
## DOCUMENTACIÓN COMPLETA DE APIs - ENDPOINTS Y PARÁMETROS

---

## ?? Índice
1. [Autenticación (Auth)](#1-autenticación-auth)
2. [Usuarios (Users)](#2-usuarios-users)
3. [Clientes (Client)](#3-clientes-client)
4. [Productos (Product)](#4-productos-product)
5. [Facturas (Invoice)](#5-facturas-invoice)
6. [Pagos (Payment)](#6-pagos-payment)
7. [Carrito de Compras (ShoppingCart)](#7-carrito-de-compras-shoppingcart)
8. [Imágenes (Images)](#8-imágenes-images)
9. [Roles](#9-roles)

---

## ?? 1. Autenticación (Auth)

### Base URL
```
/api/auth
```

---

### 1.1. Login (Iniciar Sesión)

**Endpoint:** `POST /api/auth/login`

**Descripción:** Autentica un usuario y retorna un token JWT.

**Autorización:** ? No requiere

**Request Body:**
```json
{
  "email": "string",
  "password": "string"
}
```

**Parámetros Requeridos:**
| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| email | string | Email del usuario | Requerido, formato email válido |
| password | string | Contraseña | Requerido |

**Response Success (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-12-31T23:59:59Z",
  "user": {
    "email": "admin@example.com",
"username": "admin",
    "roles": ["Administrator"]
  }
}
```

**Response Errors:**

**401 Unauthorized - Usuario no encontrado:**
```json
{
  "errorCode": "USER_NOT_FOUND",
  "message": "Invalid credentials."
}
```

**401 Unauthorized - Credenciales inválidas:**
```json
{
  "errorCode": "INVALID_CREDENTIALS",
  "message": "Invalid credentials."
}
```

**401 Unauthorized - Cuenta bloqueada:**
```json
{
  "errorCode": "ACCOUNT_LOCKED",
  "message": "La cuenta está bloqueada por demasiados intentos fallidos. Intente más tarde."
}
```

**Ejemplo con cURL:**
```bash
curl -X POST https://api.example.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "Admin@123"
  }'
```

---

## ?? 2. Usuarios (Users)

### Base URL
```
/api/users
```

---

### 2.1. Obtener Todos los Usuarios

**Endpoint:** `GET /api/users`

**Descripción:** Lista todos los usuarios del sistema.

**Autorización:** ? Requiere rol `Administrator`

**Headers:**
```
Authorization: Bearer {token}
```

**Response Success (200 OK):**
```json
[
  {
    "id": "guid-string",
    "identificationNumber": "0123456789",
    "email": "user@example.com",
    "emailConfirmed": true,
    "userName": "johndoe",
    "profileImageUri": "https://cloudinary.com/image.jpg",
    "roles": ["User"],
  "isLocked": false,
    "createdAt": "2024-01-01T00:00:00Z",
 "updatedAt": "2024-06-01T00:00:00Z"
  }
]
```

---

### 2.2. Obtener Usuario por ID

**Endpoint:** `GET /api/users/{id}`

**Descripción:** Obtiene la información de un usuario específico.

**Autorización:** ? Requiere rol `Administrator`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | string | GUID del usuario |

**Response Success (200 OK):**
```json
{
  "id": "guid-string",
  "identificationNumber": "0123456789",
  "email": "user@example.com",
  "emailConfirmed": true,
  "userName": "johndoe",
  "profileImageUri": "https://cloudinary.com/image.jpg",
  "roles": ["User"],
  "isLocked": false,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null
}
```

**Response Errors:**
- **404 Not Found:** Usuario no encontrado
- **500 Internal Server Error:** Error interno

---

### 2.3. Crear Usuario

**Endpoint:** `POST /api/users`

**Descripción:** Crea un nuevo usuario en el sistema.

**Autorización:** ? Requiere rol `Administrator`

**Content-Type:** `multipart/form-data`

**Request Body (Form Data):**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| identificationNumber | string | Cédula o pasaporte | Requerido, validación cédula EC o pasaporte |
| email | string | Correo electrónico | Requerido, formato email, máx 100 caracteres |
| userName | string | Nombre de usuario | Requerido, máx 50 caracteres |
| password | string | Contraseña | Requerido, política de contraseña fuerte |
| roles | string[] | Roles del usuario | Opcional, valores: "Administrator", "User" |
| profileImage | file | Imagen de perfil | Opcional, formatos: jpg, png, gif |

**Validación de Password:**
- Mínimo 8 caracteres
- Al menos 1 mayúscula
- Al menos 1 minúscula
- Al menos 1 número
- Al menos 1 carácter especial (@$!%*?&)

**Ejemplo Request Body:**
```
identificationNumber: 0123456789
email: newuser@example.com
userName: newuser
password: SecurePass@123
roles: ["User"]
profileImage: [archivo binario]
```

**Response Success (201 Created):**
```json
{
  "id": "new-guid-string",
  "userName": "newuser",
  "email": "newuser@example.com",
  "identification": "0123456789",
  "profileImageUri": "https://cloudinary.com/image.jpg",
  "roles": ["User"]
}
```

**Response Errors:**

**400 Bad Request - Email ya existe:**
```json
{
  "message": "Email already exists"
}
```

**400 Bad Request - Username ya existe:**
```json
{
  "message": "Username already exists"
}
```

**400 Bad Request - Roles inválidos:**
```json
{
  "message": "Invalid roles provided",
  "invalidRoles": ["InvalidRole"]
}
```

**400 Bad Request - Error de validación:**
```json
{
  "message": "Validation failed",
  "errors": {
    "Password": ["La contraseña debe contener al menos 8 caracteres"],
    "Email": ["El formato del email es inválido"]
  }
}
```

---

### 2.4. Actualizar Usuario

**Endpoint:** `PUT /api/users/{id}`

**Descripción:** Actualiza un usuario existente.

**Autorización:** ? Requiere rol `Administrator` o ser el propietario

**Content-Type:** `multipart/form-data`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | string | GUID del usuario |

**Request Body (Form Data):**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| id | string | GUID del usuario | Requerido, debe coincidir con URL |
| identificationNumber | string | Cédula o pasaporte | Requerido |
| email | string | Correo electrónico | Requerido, formato email |
| userName | string | Nombre de usuario | Requerido |
| emailConfirmed | boolean | Email confirmado | Solo Admin |
| roles | string[] | Roles del usuario | Solo Admin |
| isLocked | boolean | Usuario bloqueado | Solo Admin |
| profileImage | file | Nueva imagen de perfil | Opcional |

**Response Success (200 OK):**
```json
{
  "message": "Usuario actualizado exitosamente."
}
```

**Response Errors:**
- **400 Bad Request:** ID mismatch, email/username duplicado
- **403 Forbidden:** Sin permisos
- **404 Not Found:** Usuario no encontrado

---

### 2.5. Eliminar Usuario

**Endpoint:** `DELETE /api/users/{id}`

**Descripción:** Elimina permanentemente un usuario del sistema.

**Autorización:** ? Requiere rol `Administrator`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | string | GUID del usuario |

**Response Success (200 OK):**
```json
{
  "message": "User deleted successfully"
}
```

---

### 2.6. Desbloquear Usuario

**Endpoint:** `POST /api/users/{id}/unlock`

**Descripción:** Desbloquea un usuario bloqueado por intentos fallidos.

**Autorización:** ? Requiere rol `Administrator`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | string | GUID del usuario |

**Response Success (200 OK):**
```json
{
  "message": "User unlocked successfully"
}
```

---

### 2.7. Obtener Mi Perfil

**Endpoint:** `GET /api/users/me`

**Descripción:** Obtiene el perfil del usuario autenticado.

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Response Success (200 OK):**
```json
{
  "id": "current-user-guid",
  "identificationNumber": "0123456789",
  "email": "currentuser@example.com",
  "emailConfirmed": true,
  "userName": "currentuser",
  "profileImageUri": "https://cloudinary.com/image.jpg",
  "roles": ["User"],
  "isLocked": false,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": "2024-06-01T00:00:00Z"
}
```

---

### 2.8. Eliminar Foto de Perfil

**Endpoint:** `DELETE /api/users/{id}/profile-image`

**Descripción:** Elimina solo la imagen de perfil de un usuario.

**Autorización:** ? Requiere rol `Administrator` o ser el propietario

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | string | GUID del usuario |

**Response Success (200 OK):**
```json
{
  "message": "Profile image deleted successfully"
}
```

**Response Errors:**
- **400 Bad Request:** Usuario no tiene imagen de perfil
- **403 Forbidden:** Sin permisos
- **404 Not Found:** Usuario o imagen no encontrada

---

## ?? 3. Clientes (Client)

### Base URL
```
/api/client
```

---

### 3.1. Obtener Cliente por ID

**Endpoint:** `GET /api/client/{id}`

**Descripción:** Obtiene un cliente por su ID (solo activos).

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID del cliente |

**Response Success (200 OK):**
```json
{
  "clientId": 1,
  "identificationType": "Cedula",
  "identificationNumber": "1234567890",
  "firstName": "Juan",
  "lastName": "Pérez",
  "phone": "0987654321",
  "email": "juan.perez@example.com",
  "address": "Av. Principal 123, Quito",
  "isActive": true,
  "createdAt": "2024-01-01T00:00:00Z",
  "updatedAt": null,
  "deletedAt": null
}
```

**Response Errors:**
- **404 Not Found:** Cliente no encontrado
- **400 Bad Request:** ID inválido

---

### 3.2. Obtener Todos los Clientes

**Endpoint:** `GET /api/client`

**Descripción:** Lista todos los clientes activos con paginación y búsqueda.

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Query Parameters:**
| Parámetro | Tipo | Descripción | Default | Requerido |
|-----------|------|-------------|---------|-----------|
| pageNumber | int | Número de página | 1 | No |
| pageSize | int | Registros por página | 10 | No |
| searchTerm | string | Término de búsqueda | null | No |

**Ejemplo Request:**
```
GET /api/client?pageNumber=1&pageSize=20&searchTerm=Juan
```

**Response Success (200 OK):**
```json
{
  "items": [
    {
      "clientId": 1,
      "identificationType": "Cedula",
  "identificationNumber": "1234567890",
      "firstName": "Juan",
      "lastName": "Pérez",
  "phone": "0987654321",
      "email": "juan.perez@example.com",
      "address": "Av. Principal 123, Quito",
      "isActive": true,
      "createdAt": "2024-01-01T00:00:00Z"
    }
  ],
  "totalCount": 50,
  "pageNumber": 1,
  "pageSize": 20,
  "totalPages": 3,
  "hasNextPage": true,
  "hasPreviousPage": false,
  "firstItemIndex": 1,
  "lastItemIndex": 20,
  "isEmpty": false
}
```

---

### 3.3. Obtener Clientes Incluyendo Eliminados

**Endpoint:** `GET /api/client/all-including-deleted`

**Descripción:** Lista todos los clientes incluyendo los eliminados lógicamente.

**Autorización:** ? Requiere rol `Administrator`

**Query Parameters:**
| Parámetro | Tipo | Descripción | Default | Requerido |
|-----------|------|-------------|---------|-----------|
| pageNumber | int | Número de página | 1 | No |
| pageSize | int | Registros por página | 10 | No |
| searchTerm | string | Término de búsqueda | null | No |

---

### 3.4. Obtener Solo Clientes Eliminados

**Endpoint:** `GET /api/client/deleted`

**Descripción:** Lista solo los clientes eliminados lógicamente.

**Autorización:** ? Requiere rol `Administrator`

**Response Success (200 OK):**
```json
[
  {
    "clientId": 5,
    "firstName": "María",
    "lastName": "García",
    "identificationNumber": "0987654321",
    "isActive": false,
    "deletedAt": "2024-06-01T10:30:00Z"
  }
]
```

---

### 3.5. Crear Cliente

**Endpoint:** `POST /api/client`

**Descripción:** Crea un nuevo cliente con validaciones completas.

**Autorización:** ? Requiere rol `Administrator`

**Content-Type:** `application/json`

**Request Body:**
```json
{
"identificationType": "string",
  "identificationNumber": "string",
  "firstName": "string",
  "lastName": "string",
  "phone": "string",
  "email": "string",
  "address": "string"
}
```

**Parámetros Requeridos:**

| Campo | Tipo | Descripción | Validaciones |
|-------|------|-------------|--------------|
| identificationType | string | Tipo de identificación | Requerido, valores: "Cedula", "RUC", "Pasaporte" |
| identificationNumber | string | Número de identificación | Requerido, validación según tipo |
| firstName | string | Nombres | Requerido, 2-50 caracteres, solo letras y espacios |
| lastName | string | Apellidos | Requerido, 2-50 caracteres, solo letras y espacios |
| phone | string | Teléfono | Requerido, formato ecuatoriano (09XXXXXXXX o 02XXXXXXX) |
| email | string | Correo electrónico | Requerido, formato email válido, máx 100 caracteres |
| address | string | Dirección | Requerido, máx 200 caracteres |

**Validaciones de Identificación:**

- **Cédula:** 10 dígitos, algoritmo de validación ecuatoriano
- **RUC:** 13 dígitos, algoritmo de validación ecuatoriano
- **Pasaporte:** Formato alfanumérico internacional

**Validaciones de Teléfono Ecuatoriano:**
- Celular: `09XXXXXXXX` (10 dígitos iniciando con 09)
- Convencional: `0[2-7]XXXXXXX` (9 dígitos)

**Ejemplo Request:**
```json
{
  "identificationType": "Cedula",
  "identificationNumber": "1713175071",
  "firstName": "Carlos",
  "lastName": "Mendoza",
  "phone": "0987654321",
  "email": "carlos.mendoza@example.com",
  "address": "Calle Falsa 123, Quito, Ecuador"
}
```

**Response Success (201 Created):**
```json
{
  "message": "Client created successfully"
}
```

**Response Errors:**

**400 Bad Request - Validación fallida:**
```json
{
  "message": "Validation failed",
  "errors": {
    "IdentificationNumber": [
      "El número de cédula no es válido"
    ],
    "Phone": [
      "El número de teléfono debe ser un celular ecuatoriano válido (09XXXXXXXX)"
    ]
  }
}
```

**400 Bad Request - Cliente duplicado:**
```json
{
  "message": "Ya existe un cliente con esta identificación"
}
```

---

### 3.6. Actualizar Cliente

**Endpoint:** `PUT /api/client/{id}`

**Descripción:** Actualiza un cliente existente.

**Autorización:** ? Requiere rol `Administrator`

**Content-Type:** `application/json`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID del cliente |

**Request Body:**
```json
{
  "clientId": 1,
  "identificationType": "Cedula",
  "identificationNumber": "1713175071",
  "firstName": "Carlos",
  "lastName": "Mendoza",
  "phone": "0987654321",
  "email": "carlos.mendoza@example.com",
  "address": "Nueva Dirección 456, Quito"
}
```

**?? Importante:** El `clientId` en el body debe coincidir con el `id` de la URL.

**Response Success (200 OK):**
```json
{
  "message": "Client updated successfully"
}
```

**Response Errors:**
- **400 Bad Request:** ID mismatch o validación fallida
- **404 Not Found:** Cliente no encontrado

---

### 3.7. Eliminar Cliente (Soft Delete)

**Endpoint:** `DELETE /api/client/{id}`

**Descripción:** Realiza borrado lógico del cliente (cambia `isActive` a `false`).

**Autorización:** ? Requiere rol `Administrator`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID del cliente |

**Response Success (200 OK):**
```json
{
  "message": "Client deleted successfully (soft delete)"
}
```

---

### 3.8. Restaurar Cliente

**Endpoint:** `POST /api/client/{id}/restore`

**Descripción:** Restaura un cliente eliminado lógicamente.

**Autorización:** ? Requiere rol `Administrator`

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| id | int | ID del cliente |

**Response Success (200 OK):**
```json
{
  "message": "Client restored successfully"
}
```

---

### 3.9. Obtener Estadísticas de Clientes

**Endpoint:** `GET /api/client/statistics`

**Descripción:** Obtiene estadísticas generales de clientes.

**Autorización:** ? Requiere rol `Administrator`

**Response Success (200 OK):**
```json
{
  "activeClients": 45,
  "deletedClients": 5,
  "totalClients": 50,
  "deletionRate": 10.0
}
```

---

### 3.10. Buscar Cliente por Identificación

**Endpoint:** `GET /api/client/by-identification/{identification}`

**Descripción:** Busca un cliente por su número de identificación.

**Autorización:** ? Requiere autenticación (`Administrator` o `User`)

**Parámetros de Ruta:**
| Parámetro | Tipo | Descripción |
|-----------|------|-------------|
| identification | string | Número de cédula/RUC/pasaporte |

**Ejemplo Request:**
```
GET /api/client/by-identification/1713175071
```

**Response Success (200 OK):**
```json
{
  "clientId": 1,
  "identificationType": "Cedula",
  "identificationNumber": "1713175071",
  "firstName": "Carlos",
  "lastName": "Mendoza",
  "phone": "0987654321",
  "email": "carlos.mendoza@example.com",
  "address": "Calle Falsa 123, Quito",
  "isActive": true
}
```

**Response Errors:**
- **404 Not Found:** Cliente no encontrado

---

**Continúa en PARTE 3...**

---

**Fecha de última actualización:** $(Get-Date)
**Versión del proyecto:** .NET 9
