# API Documentation - Parte 1: Autenticación y Usuarios

## ?? Índice
- [Autenticación](#autenticación)
- [Gestión de Usuarios](#gestión-de-usuarios)

---

## ?? Autenticación

### POST `/api/Auth/login`
Autentica un usuario y devuelve un token JWT.

**Autorización:** No requerida

**Request Body:**
```json
{
  "email": "string",
  "password": "string"
}
```

**Validaciones:**
- `email`: Email válido (requerido)
- `password`: String (requerido)

**Respuestas:**

**? 200 OK** - Login exitoso
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-12-31T23:59:59Z",
  "user": {
    "email": "usuario@example.com",
    "username": "usuario",
  "roles": ["Administrator", "user"]
  }
}
```

**? 401 Unauthorized** - Credenciales inválidas
```json
{
  "errorCode": "USER_NOT_FOUND",
  "message": "Invalid credentials."
}
```
```json
{
  "errorCode": "INVALID_CREDENTIALS",
  "message": "Invalid credentials."
}
```

**? 401 Unauthorized** - Cuenta bloqueada
```json
{
  "errorCode": "ACCOUNT_LOCKED",
"message": "La cuenta está bloqueada por demasiados intentos fallidos. Intente más tarde."
}
```

---

## ?? Gestión de Usuarios

### GET `/api/Users`
Obtiene la lista completa de usuarios.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Respuesta:**

**? 200 OK**
```json
[
  {
    "id": "user-guid-1234",
    "userName": "johndoe",
    "identificationNumber": "1234567890",
    "email": "john@example.com",
    "emailConfirmed": true,
    "profileImageUri": "https://cloudinary.com/...",
    "roles": ["Administrator"],
    "isLocked": false,
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-06-15T14:30:00Z"
  }
]
```

---

### GET `/api/Users/{id}`
Obtiene un usuario específico por ID.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (string): ID del usuario (GUID)

**Respuestas:**

**? 200 OK**
```json
{
  "id": "user-guid-1234",
  "userName": "johndoe",
  "identificationNumber": "1234567890",
  "email": "john@example.com",
  "emailConfirmed": true,
  "profileImageUri": "https://cloudinary.com/...",
  "roles": ["Administrator", "user"],
  "isLocked": false,
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-06-15T14:30:00Z"
}
```

**? 404 Not Found**
```json
{
  "message": "User not found"
}
```

---

### POST `/api/Users`
Crea un nuevo usuario.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Request Body (Form-Data):**
```
identificationNumber: string (requerido) - Cédula o pasaporte
email: string (requerido) - Email válido, máx 100 caracteres
userName: string (requerido) - Máx 50 caracteres
password: string (requerido) - Contraseña fuerte (validación especial)
roles: string[] (opcional) - Ejemplo: ["Administrator", "user"]
profileImage: file (opcional) - Imagen de perfil
```

**Validaciones:**
- **identificationNumber**: 
  - Cédula ecuatoriana (10 dígitos) o pasaporte
  - Validación por atributo `[IdentificationValidation]`
- **email**: 
  - Formato de email válido
  - Único en el sistema
  - Máximo 100 caracteres
- **userName**: 
  - Único en el sistema
  - Máximo 50 caracteres
- **password**: 
  - Validación `[StrongPassword]` (debe cumplir políticas de complejidad)
- **roles**: 
  - Roles deben existir en el sistema
  - Validación contra base de datos
- **profileImage**: 
  - Archivo de imagen opcional
  - Se sube a Cloudinary

**Respuestas:**

**? 201 Created**
```json
{
  "id": "new-user-guid",
  "userName": "newuser",
  "email": "newuser@example.com",
  "identification": "1234567890",
  "profileImageUri": "https://cloudinary.com/...",
  "roles": ["user"]
}
```

**? 400 Bad Request** - Validación fallida
```json
{
  "message": "Validation failed",
  "errors": {
    "Email": ["El email ya existe"],
    "Password": ["La contraseña debe contener al menos 8 caracteres"]
  }
}
```

**? 400 Bad Request** - Email duplicado
```json
{
  "message": "Email already exists"
}
```

**? 400 Bad Request** - Username duplicado
```json
{
  "message": "Username already exists"
}
```

**? 400 Bad Request** - Roles inválidos
```json
{
  "message": "Invalid roles provided",
  "invalidRoles": ["RoleNoExistente"]
}
```

---

### PUT `/api/Users/{id}`
Actualiza un usuario existente.

**Autorización:** Administrator, user (solo puede actualizar su propio perfil)

**Headers:**
```
Authorization: Bearer {token}
Content-Type: multipart/form-data
```

**Path Parameters:**
- `id` (string): ID del usuario (GUID)

**Request Body (Form-Data):**
```
id: string (requerido) - Debe coincidir con el ID de la URL
identificationNumber: string (requerido)
email: string (requerido)
userName: string (requerido)
emailConfirmed: boolean (solo Administrator)
roles: string[] (solo Administrator)
isLocked: boolean (solo Administrator)
profileImage: file (opcional) - Nueva imagen de perfil
```

**Validaciones:**
- `id` de la URL debe coincidir con `id` del body
- Usuario solo puede actualizar su propio perfil (excepto Administrator)
- Administrator puede actualizar cualquier usuario y cambiar roles
- Validaciones similares a la creación para email, username, etc.

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Usuario actualizado exitosamente."
}
```

**? 400 Bad Request** - ID mismatch
```json
{
  "message": "ID mismatch"
}
```

**? 403 Forbidden**
```json
"No tienes permisos para actualizar este usuario."
```

**? 404 Not Found**
```json
{
  "message": "User not found"
}
```

---

### DELETE `/api/Users/{id}`
Elimina un usuario (eliminación física).

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (string): ID del usuario

**Respuestas:**

**? 200 OK**
```json
{
  "message": "User deleted successfully"
}
```

**? 404 Not Found**
```json
{
  "message": "User not found"
}
```

---

### POST `/api/Users/{id}/unlock`
Desbloquea un usuario bloqueado.

**Autorización:** Administrator

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (string): ID del usuario

**Respuestas:**

**? 200 OK**
```json
{
  "message": "User unlocked successfully"
}
```

**? 404 Not Found**
```json
{
  "message": "User not found"
}
```

---

### GET `/api/Users/me`
Obtiene el perfil del usuario autenticado.

**Autorización:** Administrator, user

**Headers:**
```
Authorization: Bearer {token}
```

**Respuestas:**

**? 200 OK**
```json
{
  "id": "current-user-guid",
  "userName": "myusername",
  "identificationNumber": "1234567890",
  "email": "myemail@example.com",
  "emailConfirmed": true,
  "profileImageUri": "https://cloudinary.com/...",
  "roles": ["user"],
  "isLocked": false,
  "createdAt": "2024-01-01T10:00:00Z",
  "updatedAt": "2024-06-15T14:30:00Z"
}
```

**? 401 Unauthorized**
```json
{
  "message": "Unauthorized"
}
```

---

### DELETE `/api/Users/{id}/profile-image`
Elimina la imagen de perfil de un usuario.

**Autorización:** Administrator, user (solo puede eliminar su propia imagen)

**Headers:**
```
Authorization: Bearer {token}
```

**Path Parameters:**
- `id` (string): ID del usuario

**Respuestas:**

**? 200 OK**
```json
{
  "message": "Profile image deleted successfully"
}
```

**? 400 Bad Request**
```json
{
  "message": "User has no profile image to delete"
}
```

**? 403 Forbidden**
```json
"No tienes permisos para eliminar esta imagen."
```

**? 404 Not Found**
```json
{
  "message": "User not found"
}
```
```json
{
  "message": "Image not found or could not be deleted"
}
```

---

## ?? Notas Importantes

### Seguridad
- Todos los endpoints requieren token JWT excepto `/api/Auth/login`
- El token se envía en el header: `Authorization: Bearer {token}`
- Los tokens expiran según configuración (`ExpiryInHours` en appsettings)

### Validaciones Personalizadas
- **[IdentificationValidation]**: Valida cédula ecuatoriana o pasaporte
- **[StrongPassword]**: Valida complejidad de contraseña
- **[EmailAddress]**: Validación estándar de email

### Manejo de Imágenes
- Las imágenes se suben a Cloudinary
- Formatos aceptados: JPG, PNG, etc.
- Si falla la creación del usuario, la imagen se elimina automáticamente
- Al actualizar/eliminar usuario, se elimina la imagen anterior

### Códigos de Error Comunes
- **400**: Validación fallida o datos incorrectos
- **401**: No autenticado o credenciales inválidas
- **403**: No autorizado (falta permisos)
- **404**: Recurso no encontrado
- **500**: Error interno del servidor
