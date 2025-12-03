# ?? DOCUMENTACIÓN DEL PROYECTO - PARTE 1
## INTRODUCCIÓN Y ARQUITECTURA DEL SISTEMA

---

## ?? Índice
1. [Visión General del Proyecto](#visión-general-del-proyecto)
2. [Arquitectura del Sistema](#arquitectura-del-sistema)
3. [Tecnologías Utilizadas](#tecnologías-utilizadas)
4. [Estructura del Proyecto](#estructura-del-proyecto)
5. [Patrones de Diseño](#patrones-de-diseño)

---

## ?? Visión General del Proyecto

Este proyecto es un **Sistema de Gestión Empresarial** desarrollado con **.NET 9** que implementa una arquitectura limpia y escalable para la administración de:

- ?? **Usuarios y Autenticación** con JWT
- ?? **Productos e Inventario** con gestión de imágenes
- ?? **Clientes** con validaciones específicas para Ecuador
- ?? **Facturas** con sistema de numeración automática
- ?? **Pagos** y métodos de pago
- ??? **Carritos de compra**

### Características Principales

? **Autenticación y Autorización**
- Sistema JWT para tokens de acceso
- Roles: Administrator y User
- Bloqueo de cuentas por intentos fallidos

? **Gestión de Imágenes**
- Integración con Cloudinary
- Carga de imágenes para productos y perfiles

? **Soft Delete (Borrado Lógico)**
- Todos los registros mantienen historial
- Posibilidad de restauración

? **Validaciones Específicas para Ecuador**
- Validación de cédulas ecuatorianas
- Validación de RUC
- Validación de números telefónicos ecuatorianos

? **Paginación**
- Todos los listados soportan paginación
- Búsqueda integrada en endpoints

---

## ??? Arquitectura del Sistema

El proyecto sigue una **Arquitectura en Capas** (Clean Architecture):

```
???????????????????????????????????????????????????????????????
?        API LAYER (Api)       ?
?  - Controllers         ?
?  - Middlewares     ?
?  - JWT Configuration     ?
???????????????????????????????????????????????????????????????
             ?
???????????????????????????????????????????????????????????????
?        APPLICATION LAYER (Project.Application)     ?
?  - DTOs (Data Transfer Objects)            ?
?  - Services    ?
?  - Validators    ?
?  - Use Cases           ?
???????????????????????????????????????????????????????????????
        ?
???????????????????????????????????????????????????????????????
?      DOMAIN LAYER (Project.Domain)     ?
?  - Entities         ?
?  - Value Objects    ?
?  - Domain Exceptions          ?
?  - Repository Interfaces?
???????????????????????????????????????????????????????????????
        ?
???????????????????????????????????????????????????????????????
?          INFRASTRUCTURE (Project.Infrastructure)            ?
?  - Database Context (EF Core)         ?
?  - Repository Implementations    ?
?  - Identity Framework             ?
?  - External Services (Cloudinary)      ?
???????????????????????????????????????????????????????????????
```

---

## ??? Tecnologías Utilizadas

### Backend
- **.NET 9** (C# 13.0)
- **ASP.NET Core Web API**
- **Entity Framework Core** (ORM)
- **Microsoft Identity** (Autenticación)
- **JWT Bearer** (Tokens)

### Base de Datos
- **SQL Server** (Producción)
- **Entity Framework Core Migrations**

### Servicios Externos
- **Cloudinary** (Almacenamiento de imágenes)

### Seguridad
- **JWT Tokens**
- **Password Hashing** (Identity)
- **Role-Based Authorization**
- **CORS Configuration**

### Validaciones
- **Data Annotations**
- **FluentValidation**
- **Custom Validators**

---

## ?? Estructura del Proyecto

```
ProjectFinal/
?
??? Api/
?   ??? Controllers/
?   ?   ??? AuthController.cs          # Autenticación y login
? ?   ??? UsersController.cs         # Gestión de usuarios
?   ?   ??? ClientController.cs        # Gestión de clientes
?   ?   ??? ProductController.cs     # Gestión de productos
?   ?   ??? InvoiceController.cs       # Gestión de facturas
?   ?   ??? PaymentController.cs       # Gestión de pagos
?   ?   ??? ShoppingCartController.cs  # Carrito de compras
?   ?   ??? ImagesController.cs      # Gestión de imágenes
?   ?   ??? RolesController.cs         # Gestión de roles
?   ??? Program.cs           # Configuración principal
?
??? Project.Application/
? ??? Dtos/            # Objetos de transferencia
?   ??? Services/        # Lógica de negocio
?   ??? Validators/            # Validaciones personalizadas
?   ??? Interfaces/ # Contratos de servicios
?   ??? UseCases/      # Casos de uso CQRS
?
??? Project.Domain/
?   ??? Entities/    # Entidades del dominio
?   ??? ValueObjects/      # Objetos de valor
?   ??? Interfaces/   # Contratos de repositorios
?   ??? Exceptions/  # Excepciones del dominio
?
??? Project.Infrastructure/
    ??? Data/       # Contexto de BD
 ??? Repositories/    # Implementación de repositorios
    ??? Frameworks/
    ?   ??? Identity/            # Configuración de Identity
    ??? Services/        # Servicios de infraestructura
```

---

## ?? Patrones de Diseño

### 1. **Repository Pattern**
Abstracción de la capa de acceso a datos.

```csharp
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
  Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}
```

### 2. **Unit of Work**
Coordinación de múltiples repositorios en una transacción.

```csharp
public interface IUnitOfWork : IDisposable
{
    IClientRepository Clients { get; }
    IProductRepository Products { get; }
 IInvoiceRepository Invoices { get; }
    Task<int> SaveChangesAsync();
}
```

### 3. **DTO Pattern**
Separación entre entidades de dominio y objetos de transferencia.

### 4. **Service Layer**
Encapsulación de la lógica de negocio.

### 5. **Dependency Injection**
Inversión de control para mejor testabilidad.

### 6. **CQRS (Command Query Responsibility Segregation)**
Separación de comandos y consultas (parcialmente implementado).

---

## ?? Configuración de Seguridad

### JWT Configuration
```json
{
  "Jwt": {
    "Key": "your-secret-key-here",
    "Issuer": "your-issuer",
  "Audience": "your-audience",
    "ExpiryInHours": 24
  }
}
```

### Roles del Sistema
- **Administrator**: Acceso completo a todas las funcionalidades
- **User**: Acceso limitado a operaciones de lectura y creación

### Políticas de Password
- Mínimo 8 caracteres
- Al menos 1 mayúscula
- Al menos 1 minúscula
- Al menos 1 número
- Al menos 1 carácter especial
- Historial de contraseñas (no repetir últimas 5)

---

## ?? Modelo de Datos Principal

### Entidades Principales

1. **ApplicationUser** (Identity)
   - Información del usuario
   - Roles y permisos
- Perfil e imagen

2. **Client**
   - Información del cliente
   - Validación de identificación ecuatoriana
   - Soft delete

3. **Product**
   - Inventario de productos
   - Precios y stock
   - Imágenes en Cloudinary

4. **Invoice**
   - Facturación automática
   - Detalles de productos
   - Estados (Draft, Finalized, Cancelled)

5. **Payment**
   - Registro de pagos
   - Métodos de pago
   - Relación con facturas

---

## ?? Próximos Pasos

En la **PARTE 2** se documentarán todos los endpoints de la API con:
- Métodos HTTP
- Parámetros requeridos
- Ejemplos de requests y responses
- Códigos de estado HTTP

---

**Fecha de última actualización:** $(Get-Date)
**Versión del proyecto:** .NET 9
**Autor:** Equipo de Desarrollo
