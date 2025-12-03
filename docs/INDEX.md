# ?? DOCUMENTACIÓN COMPLETA DEL PROYECTO

## Sistema de Gestión Empresarial - .NET 9

---

## ?? Índice de Documentación

Esta documentación está dividida en **4 partes** para facilitar su consulta y comprensión:

### **[PARTE 1: Introducción y Arquitectura](PARTE_1_INTRODUCCION_Y_ARQUITECTURA.md)**
- ?? Visión general del proyecto
- ??? Arquitectura del sistema (Clean Architecture)
- ??? Tecnologías utilizadas (.NET 9, EF Core, JWT, Cloudinary)
- ?? Estructura del proyecto
- ?? Patrones de diseño implementados
- ?? Configuración de seguridad
- ?? Modelo de datos principal

**Ideal para:** Desarrolladores nuevos, arquitectos, revisión de estructura

---

### **[PARTE 2: APIs - Autenticación, Usuarios y Clientes](PARTE_2_APIS_AUTH_USERS_CLIENTS.md)**
- ?? **Autenticación (Auth)**
  - Login con JWT
  - Manejo de bloqueos de cuenta
  
- ?? **Usuarios (Users)**
  - CRUD completo de usuarios
  - Gestión de roles
  - Subida de fotos de perfil
  - Desbloqueo de cuentas
  
- ?? **Clientes (Client)**
  - CRUD de clientes
  - Validaciones específicas para Ecuador (cédula, RUC, teléfono)
  - Soft delete y restauración
  - Búsqueda y estadísticas

**Ideal para:** Implementación de autenticación, gestión de usuarios y clientes

---

### **[PARTE 3: APIs - Productos y Facturas](PARTE_3_APIS_PRODUCTS_INVOICES.md)**
- ?? **Productos (Product)**
  - CRUD de productos con imágenes
  - Gestión de stock
  - Productos con stock bajo
  - Búsqueda por código
  - Soft delete y restauración
  
- ?? **Facturas (Invoice)**
  - Creación de facturas con numeración automática
  - Cálculo automático de impuestos (IVA 12%)
  - Estados: Draft, Finalized, Cancelled
  - Búsqueda avanzada
  - Detalles de factura

**Ideal para:** Gestión de inventario y facturación

---

### **[PARTE 4: APIs - Pagos, Carrito y Servicios Adicionales](PARTE_4_APIS_PAGOS_CARRITO_COMPLETO.md)**
- ?? **Pagos (Payment)**
  - Procesamiento de pagos (simulación)
  - Pagos móviles
  - Historial de pagos
  - Reembolsos (admin)
  - Validación de montos
  
- ?? **Métodos de Pago**
  - Tarjetas de crédito/débito
  - Billeteras móviles
  - Transferencias bancarias
  
- ??? **Carrito de Compras**
  - Agregar/actualizar/eliminar productos
  - Convertir carrito en factura (checkout)
  
- ?? **Gestión de Imágenes**
  - Subida a Cloudinary
  - Eliminación de imágenes
  
- ?? **Roles**
  - CRUD de roles del sistema
  
- ?? **Códigos de Estado HTTP**
- ?? **Modelo de Datos Completo**
- ?? **Ejemplos de Integración Completos**

**Ideal para:** Integración de pagos, carrito de compras, flujos completos

---

## ?? Inicio Rápido

### 1. **Configuración Inicial**

**Requisitos:**
- .NET 9 SDK
- SQL Server
- Cuenta de Cloudinary (para imágenes)

**Configurar appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ProjectDB;Trusted_Connection=True;"
  },
  "Jwt": {
    "Key": "your-secret-key-minimum-32-characters-long",
    "Issuer": "ProjectAPI",
    "Audience": "ProjectClient",
    "ExpiryInHours": 24
  },
  "CloudinarySettings": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

---

### 2. **Ejecutar Migraciones**

```bash
cd Api
dotnet ef database update --project ../Project.Infrastructure
```

---

### 3. **Ejecutar la API**

```bash
dotnet run --project Api
```

La API estará disponible en: `https://localhost:5001` o `http://localhost:5000`

---

### 4. **Swagger Documentation**

Una vez ejecutada la API, visitar:
```
https://localhost:5001/swagger
```

---

## ?? Credenciales de Prueba

### Administrator
```
Email: admin@example.com
Password: Admin@123
```

### User
```
Email: user@example.com
Password: User@123
```

---

## ?? Ejemplo de Uso Básico

### 1. Autenticarse

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "Admin@123"
  }'
```

**Response:**
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

---

### 2. Consultar Productos

```bash
curl -X GET "https://localhost:5001/api/product?pageNumber=1&pageSize=10" \
  -H "Authorization: Bearer {token}"
```

---

### 3. Crear Cliente

```bash
curl -X POST https://localhost:5001/api/client \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "identificationType": "Cedula",
    "identificationNumber": "1713175071",
    "firstName": "Juan",
    "lastName": "Pérez",
    "phone": "0987654321",
    "email": "juan.perez@example.com",
    "address": "Quito, Ecuador"
  }'
```

---

## ?? Roles y Permisos

### **Administrator**
? Acceso completo a todas las funcionalidades  
? Crear/Editar/Eliminar usuarios, clientes, productos  
? Ver estadísticas y reportes  
? Gestionar roles  
? Procesar reembolsos

### **User**
? Ver productos, clientes, facturas  
? Crear facturas  
? Procesar pagos  
? Gestionar carrito propio  
? Ver su propio perfil  
? No puede crear/editar usuarios, productos, clientes  
? No puede eliminar facturas

---

## ?? Estructura de Respuestas

### Respuesta Exitosa
```json
{
  "message": "Operation successful",
  "data": { ... }
}
```

### Respuesta con Paginación
```json
{
  "items": [ ... ],
  "totalCount": 100,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 10,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

### Respuesta de Error
```json
{
  "message": "Error description",
  "errorCode": "ERROR_CODE",
  "errors": {
    "Field1": ["Error message 1"],
    "Field2": ["Error message 2"]
  }
}
```

---

## ??? Validaciones Especiales

### Cédula Ecuatoriana
- 10 dígitos
- Algoritmo de validación módulo 10
- Ejemplo válido: `1713175071`

### RUC Ecuatoriano
- 13 dígitos
- Termina en `001`
- Ejemplo válido: `1713175071001`

### Teléfono Ecuatoriano
- **Celular:** `09XXXXXXXX` (10 dígitos)
- **Convencional:** `0[2-7]XXXXXXX` (9 dígitos)
- Ejemplos válidos: `0987654321`, `022345678`

### Contraseña Fuerte
- Mínimo 8 caracteres
- Al menos 1 mayúscula
- Al menos 1 minúscula
- Al menos 1 número
- Al menos 1 carácter especial (@$!%*?&)
- No repetir últimas 5 contraseñas

---

## ?? Testing

### Con Postman
Importar colección desde: `/docs/postman_collection.json`

### Con cURL
Ver ejemplos en cada parte de la documentación

### Con Swagger
Acceder a `https://localhost:5001/swagger` para testing interactivo

---

## ?? Dependencias Principales

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.0" />
<PackageReference Include="CloudinaryDotNet" Version="1.26.2" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
```

---

## ?? Troubleshooting

### Error: "No se puede conectar a la base de datos"
- Verificar connection string en `appsettings.json`
- Asegurar que SQL Server esté ejecutándose
- Ejecutar migraciones: `dotnet ef database update`

### Error: "Unauthorized 401"
- Verificar que el token JWT esté incluido en el header
- Verificar que el token no haya expirado
- Formato correcto: `Authorization: Bearer {token}`

### Error: "Forbidden 403"
- El usuario no tiene el rol necesario
- Verificar roles en el endpoint de documentación
- Usuario `Administrator` tiene acceso completo

### Error al subir imágenes
- Verificar configuración de Cloudinary en `appsettings.json`
- Verificar que el archivo sea jpg, png o gif
- Tamaño máximo: 10MB

---

## ?? Glosario

- **JWT:** JSON Web Token - Sistema de autenticación basado en tokens
- **Soft Delete:** Borrado lógico que marca registros como inactivos sin eliminarlos
- **DTO:** Data Transfer Object - Objeto para transferir datos entre capas
- **CRUD:** Create, Read, Update, Delete - Operaciones básicas
- **IVA:** Impuesto al Valor Agregado - 12% en Ecuador
- **Cloudinary:** Servicio cloud para almacenamiento de imágenes

---

## ?? Soporte

Para consultas o problemas:
- Revisar la documentación completa en las 4 partes
- Consultar Swagger en `/swagger`
- Contactar al equipo de desarrollo

---

## ?? Changelog

### Versión 1.0 (2024-01-15)
- ? Sistema de autenticación JWT
- ? Gestión completa de usuarios con roles
- ? Gestión de clientes con validaciones ecuatorianas
- ? Gestión de productos con imágenes
- ? Sistema de facturación con numeración automática
- ? Procesamiento de pagos con simulación
- ? Carrito de compras
- ? Soft delete en todas las entidades
- ? Paginación en todos los listados
- ? Integración con Cloudinary

---

## ?? Licencia

Este proyecto es privado y confidencial. Todos los derechos reservados.

---

**Última actualización:** 2024-01-15  
**Versión:** 1.0
**.NET Version:** 9.0  
**C# Version:** 13.0
