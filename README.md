# ProjectFinal - Clean Architecture

BackEnd del proyecto final refactorizado siguiendo los principios de **Clean Architecture** con soporte para **PostgreSQL**.

## 📋 Tabla de Contenidos
- [Descripción](#descripción)
- [Arquitectura](#arquitectura)
- [Tecnologías](#tecnologías)
- [Requisitos](#requisitos)
- [Instalación](#instalación)
- [Uso](#uso)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Migración a PostgreSQL](#migración-a-postgresql)
- [Testing](#testing)
- [Contribuciones](#contribuciones)
- [Autor](#autor)

## 📖 Descripción

Este proyecto ha sido **refactorizado a Clean Architecture** para mejorar la mantenibilidad, testabilidad y escalabilidad. Implementa:

- ✅ **Clean Architecture** con separación correcta de capas
- ✅ **Domain-Driven Design (DDD)** con Value Objects y entidades ricas
- ✅ **Repository Pattern** + **Unit of Work**
- ✅ **CQRS básico** con handlers para comandos y consultas
- ✅ **SQL Server** como base de datos principal
- ✅ **PostgreSQL** preparado para migración futura
- ✅ **Validaciones de dominio** en las entidades
- ✅ **Excepciones de dominio** personalizadas

## 🏗️ Arquitectura

```
ProjectFinal/
├── Project.Domain/           ← Núcleo (0 dependencias)
│   ├── Entities/            ← Entidades con lógica de negocio
│   ├── ValueObjects/        ← Value Objects (ej: Identification)
│   ├── Interfaces/          ← Contratos del dominio
│   ├── Exceptions/          ← Excepciones de dominio
│   └── Services/            ← Servicios de dominio
├── Project.Application/      ← Casos de uso (solo depende de Domain)
│   ├── Services/            ← Servicios de aplicación
│   ├── DTOs/                ← Data Transfer Objects
│   ├── UseCases/            ← Casos de uso específicos
│   ├── Interfaces/          ← Contratos de aplicación
│   └── Common/              ← Utilidades comunes
├── Project.Infrastructure/   ← Implementaciones (depende de Application/Domain)
│   ├── Persistence/         ← Configuraciones de EF
│   ├── Repositories/        ← Implementación de repositorios
│   ├── Frameworks/          ← Entity Framework, Identity
│   └── Extensions/          ← Extensiones de infraestructura
└── Api/                     ← Presentación (API Controllers)
    ├── Controllers/         ← Controladores de API
    └── Configuration/       ← Configuración de la API
```

### 🔄 Flujo de Dependencias
```
Api → Application → Domain
  ↘     ↓
   Infrastructure
```

## 🚀 Tecnologías

### Backend Core
- **C# 13** (.NET 9)
- **ASP.NET Core** (Web API)
- **Entity Framework Core 9** (ORM)

### Base de Datos
- **SQL Server** (Actual)
- **PostgreSQL** (Preparado para migración)

### Autenticación & Seguridad
- **ASP.NET Core Identity**
- **JWT Bearer Tokens**
- **Validación de contraseñas customizada**

### Arquitectura & Patrones
- **Clean Architecture**
- **Repository Pattern**
- **Unit of Work Pattern**
- **CQRS** (básico)
- **Domain-Driven Design**

### Testing
- **xUnit** (preparado)
- **FluentAssertions** (preparado)
- **Moq** (preparado)

## 📋 Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download) versión 9.0 o superior
- [SQL Server](https://www.microsoft.com/sql-server/) (actual)
- [PostgreSQL](https://www.postgresql.org/) (para migración futura)
- Editor de código (Visual Studio, VS Code, Rider)

## 🔧 Instalación

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/daniel-devlp/ProjectFinal.git
   cd ProjectFinal
   ```

2. **Restaurar paquetes:**
   ```bash
   dotnet restore
   ```

3. **Configurar cadena de conexión:**
   
   Actualizar `Api/appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=InvoiceDB;Trusted_Connection=true;"
     }
   }
   ```

4. **Ejecutar migraciones:**
   ```bash
   dotnet ef database update --project Project.Infrastructure --startup-project Api
   ```

5. **Iniciar el servidor:**
   ```bash
   dotnet run --project Api
   ```

## 🎯 Uso

### API Base
La API estará disponible en `https://localhost:5001`

### Swagger Documentation
Acceder a `https://localhost:5001/swagger` para la documentación interactiva.

### Endpoints Principales

#### Autenticación
```
POST /api/auth/login
POST /api/auth/register
```

#### Clientes (Clean Architecture)
```
GET    /api/clients          # Paginado con búsqueda
GET    /api/clients/{id}     # Por ID
POST   /api/clients          # Crear (con validaciones de dominio)
PUT    /api/clients/{id}     # Actualizar
DELETE /api/clients/{id}     # Eliminar
```

#### Productos
```
GET    /api/products         # Paginado con búsqueda
POST   /api/products         # Crear
PUT    /api/products/{id}    # Actualizar
DELETE /api/products/{id}    # Eliminar
```

#### Facturas (Con Unit of Work)
```
GET    /api/invoices         # Paginado con búsqueda
POST   /api/invoices         # Crear (transaccional)
PUT    /api/invoices/{id}    # Actualizar (transaccional)
DELETE /api/invoices/{id}    # Eliminar (transaccional)
```

## 📁 Estructura Detallada del Proyecto

### Domain Layer (Núcleo)
```
Project.Domain/
├── Entities/
│   ├── Client.cs           ← Entidad rica con validaciones
│   ├── Product.cs          ← Entidad de producto
│   ├── Invoice.cs          ← Entidad de factura
│   └── InvoiceDetail.cs    ← Detalle de factura
├── ValueObjects/
│   └── Identification.cs   ← Value Object para identificación
├── Interfaces/
│   ├── IRepository.cs      ← Interfaz genérica
│   ├── IUnitOfWork.cs      ← Unit of Work
│   ├── IClientRepository.cs
│   ├── IProductRepository.cs
│   └── IInvoiceRepository.cs
└── Exceptions/
    ├── DomainException.cs
    └── ClientDomainException.cs
```

### Application Layer (Casos de Uso)
```
Project.Application/
├── Services/
│   ├── ClientServices.cs   ← Servicio refactorizado
│   ├── ProductService.cs   ← Servicio refactorizado
│   └── InvoiceService.cs   ← Servicio refactorizado
├── DTOs/
│   └── Common/
│       └── PagedResultDto.cs
├── UseCases/
│   └── Clients/
│       └── Commands/
└── Interfaces/
    ├── ICommandHandler.cs
    └── IQueryHandler.cs
```

### Infrastructure Layer (Implementaciones)
```
Project.Infrastructure/
├── Repositories/
│   ├── Repository.cs       ← Repositorio genérico
│   ├── ClientRepository.cs ← Implementación específica
│   ├── ProductRepository.cs
│   ├── InvoiceRepository.cs
│   └── UnitOfWork.cs       ← Unit of Work implementado
├── Persistence/
│   └── Configuraciones/
│       └── ClientConfiguration.cs ← EF Configuration
├── Frameworks/
│   ├── EntityFramework/
│   │   └── ApplicationDBContext.cs
│   └── Identity/
└── Extensions/
    └── StringExtensions.cs
```

## 🐘 Migración a PostgreSQL

El proyecto está **preparado para migrar a PostgreSQL**. La configuración está comentada y lista:

### 1. Descomentar en `Program.cs`:
```csharp
// PostgreSQL Database Context (preparado para futura migración)
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQLConnection"),
        b => b.MigrationsAssembly("Project.Infrastructure"))
);
```

### 2. Descomentar en `Project.Infrastructure.csproj`:
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="9.0.2" />
```

### 3. Activar configuración en `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "PostgreSQLConnection": "Host=localhost;Database=ProjectFinalDB;Username=postgres;Password=yourpassword;Port=5432"
  }
}
```

### 4. Ejecutar nueva migración:
```bash
dotnet ef migrations add InitialPostgreSQL --project Project.Infrastructure --startup-project Api
dotnet ef database update --project Project.Infrastructure --startup-project Api
```

## 🧪 Testing

### Estructura de Testing Preparada
```
ProjectFinal.Tests/
├── Domain/
│   ├── Entities/
│   │   └── ClientTests.cs
│   └── ValueObjects/
│       └── IdentificationTests.cs
├── Application/
│   └── Services/
│       └── ClientServiceTests.cs
└── Infrastructure/
    └── Repositories/
        └── ClientRepositoryTests.cs
```

### Ejecutar Pruebas
```bash
dotnet test ProjectFinal.Tests
```

## ✨ Beneficios de Clean Architecture

### ✅ **Antes vs Después**

| Aspecto | Antes | Después |
|---------|-------|---------|
| **Dependencias** | ❌ Application → Infrastructure | ✅ Application → Domain |
| **Repositorios** | ❌ SaveChanges() en cada método | ✅ Unit of Work pattern |
| **Validaciones** | ❌ En servicios y controladores | ✅ En entidades de dominio |
| **Transacciones** | ❌ Manual en cada servicio | ✅ Unit of Work automático |
| **Testabilidad** | ❌ Acoplado a infraestructura | ✅ Completamente testeable |
| **Mantenibilidad** | ❌ Difícil de mantener | ✅ Fácil de mantener |

### 🎯 **Ventajas Conseguidas**

1. **Separación de Responsabilidades**: Cada capa tiene una responsabilidad específica
2. **Testabilidad**: Lógica de negocio completamente testeable
3. **Flexibilidad**: Fácil cambio de base de datos (SQL Server ↔ PostgreSQL)
4. **Mantenibilidad**: Código más limpio y organizado
5. **Escalabilidad**: Arquitectura preparada para crecer
6. **Domain-Driven Design**: Value Objects y entidades ricas

## 🤝 Contribuciones

¡Las contribuciones son bienvenidas! Por favor:

1. Fork el proyecto
2. Crear una rama para tu feature (`git checkout -b feature/nueva-caracteristica`)
3. Commit tus cambios (`git commit -am 'Agregar nueva característica'`)
4. Push a la rama (`git push origin feature/nueva-caracteristica`)
5. Abrir un Pull Request

### Estándares de Código
- Seguir principios de Clean Architecture
- Incluir pruebas unitarias
- Documentar métodos públicos
- Usar convenciones de C#

## 👨‍💻 Autor

**Daniel-devlp**
- GitHub: [@daniel-devlp](https://github.com/daniel-devlp)
- Email: [tu-email@ejemplo.com](mailto:tu-email@ejemplo.com)

---

> 🏗️ **Proyecto refactorizado a Clean Architecture** para la materia de desarrollo web.
> 
> 📚 **Incluye**: DDD, Repository Pattern, Unit of Work, Value Objects, y más.

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Consulta el archivo [LICENSE](LICENSE) para más detalles.
