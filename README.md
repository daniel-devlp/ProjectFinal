# ProjectFinal - Clean Architecture con Carrito de Compras

BackEnd del proyecto final refactorizado siguiendo los principios de **Clean Architecture** con **sistema de carrito de compras** y **módulo de pagos preparado**.

## 📋 Tabla de Contenidos
- [Descripción](#descripción)
- [Arquitectura](#arquitectura)
- [Tecnologías](#tecnologías)
- [Nuevas Funcionalidades](#nuevas-funcionalidades)
- [Requisitos](#requisitos)
- [Instalación](#instalación)
- [Uso](#uso)
- [API Endpoints](#api-endpoints)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Migración a PostgreSQL](#migración-a-postgresql)
- [Testing](#testing)
- [Contribuciones](#contribuciones)
- [Autor](#autor)

## 📖 Descripción

Este proyecto ha sido **refactorizado a Clean Architecture** e incluye un **sistema completo de carrito de compras** con **módulo de pagos preparado** para futuras implementaciones. 

### ✨ Funcionalidades implementadas:

- ✅ **Clean Architecture** con separación correcta de capas
- ✅ **Sistema de Carrito de Compras** completo
- ✅ **Checkout** (conversión de carrito a factura)
- ✅ **Gestión de Stock** automática
- ✅ **Domain-Driven Design (DDD)** con Value Objects y entidades ricas
- ✅ **Repository Pattern** + **Unit of Work**
- ✅ **CQRS básico** con handlers para comandos y consultas
- ✅ **SQL Server** como base de datos principal
- ✅ **PostgreSQL** preparado para migración futura
- ✅ **Módulo de Pagos** preparado (comentado)

## 🏗️ Arquitectura

```
ProjectFinal/
├── Project.Domain/      ← Núcleo (0 dependencias)
│   ├── Entities/            ← Client, Product, Invoice, ShoppingCart, Payment*
│   ├── ValueObjects/        ← Value Objects (ej: Identification)
│   ├── Interfaces/          ← Contratos del dominio
│   ├── Exceptions/  ← Excepciones de dominio
│   └── Services/            ← Servicios de dominio
├── Project.Application/      ← Casos de uso (solo depende de Domain)
│   ├── Services/    ← ClientService, ProductService, ShoppingCartService
│   ├── DTOs/                ← Data Transfer Objects + CartSummaryDto
│   ├── UseCases/            ← Casos de uso específicos
│   ├── Interfaces/          ← Contratos de aplicación
│   └── Common/              ← Utilidades comunes
├── Project.Infrastructure/   ← Implementaciones (depende de Application/Domain)
│   ├── Persistence/  ← Configuraciones de EF (ShoppingCart, Payment*)
│   ├── Repositories/        ← ShoppingCartRepository, PaymentRepository*
│   ├── Frameworks/          ← Entity Framework, Identity
│   └── Extensions/      ← Extensiones de infraestructura
└── Api/         ← Presentación (API Controllers)
    ├── Controllers/     ← Controllers + ShoppingCartController, PaymentController*
    └── Configuration/       ← Configuración de la API
```

*Preparado para implementación futura (comentado)

## 🚀 Tecnologías

### Backend Core
- **C# 13** (.NET 9)
- **ASP.NET Core** (Web API)
- **Entity Framework Core 9** (ORM)

### Nuevas Funcionalidades
- **🛒 Sistema de Carrito de Compras**
- **💳 Módulo de Pagos** (preparado)
- **📦 Gestión de Stock** automática
- **🔄 Checkout** transaccional

### Base de Datos
- **SQL Server** (Actual)
- **PostgreSQL** (Preparado para migración)
- **Nuevas tablas**: `ShoppingCart`, `Payments*`, `PaymentMethods*`

## ✨ Nuevas Funcionalidades

### 🛒 **Sistema de Carrito de Compras**

#### Entidad ShoppingCart
```csharp
public class ShoppingCart
{
    public int CartId { get; set; }
    public string UserId { get; set; }     // FK a AspNetUsers
    public int ProductId { get; set; }     // FK a Products
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

#### Funcionalidades del Carrito
- ✅ **Agregar productos** al carrito
- ✅ **Actualizar cantidades** de productos
- ✅ **Eliminar productos** del carrito
- ✅ **Limpiar carrito** completo
- ✅ **Obtener resumen** del carrito (items, total, cantidad)
- ✅ **Verificación de stock** automática
- ✅ **Checkout transaccional** (carrito → factura)

### 💳 **Módulo de Pagos (Preparado)**

#### Entidades Preparadas (Comentadas)
```csharp
// Payment Entity
public class Payment
{
    public int PaymentId { get; set; }
    public int InvoiceId { get; set; }
    public string PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string TransactionId { get; set; }
    public PaymentStatus Status { get; set; }
    // ... más propiedades
}

// PaymentMethod Entity
public class PaymentMethod
{
    public string PaymentMethodId { get; set; }
    public string Name { get; set; }
    public PaymentType Type { get; set; }
    // ... más propiedades
}
```

#### Métodos de Pago Preparados
- 💳 **Tarjeta de Crédito**
- 💳 **Tarjeta de Débito** 
- 🏦 **Transferencia Bancaria**
- 💵 **Efectivo**
- 🌐 **PayPal** (preparado)
- ⚡ **Stripe** (preparado)

## 🎯 API Endpoints

### 🛒 **Carrito de Compras**
```http
GET    /api/shoppingcart         # Obtener carrito del usuario
POST   /api/shoppingcart/add # Agregar producto al carrito
PUT    /api/shoppingcart/update       # Actualizar cantidad
DELETE /api/shoppingcart/remove/{id}  # Eliminar producto
DELETE /api/shoppingcart/clear        # Limpiar carrito
GET    /api/shoppingcart/count        # Obtener cantidad de items
GET    /api/shoppingcart/total        # Obtener total del carrito
GET    /api/shoppingcart/exists/{id}  # Verificar si producto existe
POST/api/shoppingcart/checkout     # Procesar checkout
```

### 💳 **Pagos (Preparados - Comentados)**
```http
# POST   /api/payment/process          # Procesar pago
# GET    /api/payment/status/{txnId}   # Estado del pago
# GET    /api/payment/methods          # Métodos disponibles
# GET    /api/payment/history          # Historial de pagos
# POST   /api/payment/refund      # Procesar reembolso
```

### 📦 **Productos (Actualizados)**
```http
GET    /api/products# Listar productos
GET    /api/products/{id}             # Obtener producto
POST   /api/products         # Crear producto (Admin)
PUT    /api/products/{id}   # Actualizar producto (Admin)  
DELETE /api/products/{id}         # Eliminar producto (Admin)
```

### 📋 **Facturas**
```http
GET    /api/invoices# Listar facturas
POST   /api/invoices      # Crear factura (desde carrito)
GET    /api/invoices/{id}   # Obtener factura
PUT/api/invoices/{id}             # Actualizar factura
DELETE /api/invoices/{id}             # Eliminar factura
```

## 🔄 Flujo del Carrito de Compras

### 1. **Usuario Navega Productos**
```http
GET /api/products?pageNumber=1&pageSize=10
```

### 2. **Agregar al Carrito**
```json
POST /api/shoppingcart/add
{
  "productId": 1,
  "quantity": 2
}
```

### 3. **Ver Carrito**
```json
GET /api/shoppingcart
// Respuesta:
{
  "items": [...],
  "total": 150.00,
  "totalItems": 5,
  "uniqueProducts": 3
}
```

### 4. **Checkout (Carrito → Factura)**
```json
POST /api/shoppingcart/checkout
{
  "clientId": 1
}
```

### 5. **Carrito se Limpia Automáticamente**
- ✅ Se crea la factura
- ✅ Se actualiza el stock
- ✅ Se limpia el carrito
- ✅ Transacción completa

## 💾 Base de Datos

### Nuevas Tablas

#### ShoppingCart
```sql
CREATE TABLE ShoppingCart (
    CartId int IDENTITY(1,1) PRIMARY KEY,
    UserId nvarchar(450) NOT NULL,
 ProductId int NOT NULL,
    Quantity int NOT NULL,
    UnitPrice decimal(18,2) NOT NULL,
    Subtotal decimal(18,2) NOT NULL,
    DateAdded datetime2 NOT NULL,
    UpdatedAt datetime2 NULL,
    
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
  UNIQUE (UserId, ProductId)
);
```

#### Pagos (Preparado - Comentado)
```sql
-- CREATE TABLE Payments (...)
-- CREATE TABLE PaymentMethods (...)
```

## 🚀 Instalación y Uso

### 1. **Clonar y Restaurar**
```bash
git clone https://github.com/daniel-devlp/ProjectFinal.git
cd ProjectFinal
dotnet restore
```

### 2. **Aplicar Migraciones**
```bash
dotnet ef database update --project Project.Infrastructure --startup-project Api
```

### 3. **Ejecutar**
```bash
dotnet run --project Api
```

### 4. **Probar Carrito**
1. **Autenticarse**: `POST /api/auth/login`
2. **Ver productos**: `GET /api/products`
3. **Agregar al carrito**: `POST /api/shoppingcart/add`
4. **Ver carrito**: `GET /api/shoppingcart`
5. **Hacer checkout**: `POST /api/shoppingcart/checkout`

## 🧪 Testing del Carrito

### Casos de Uso para Probar

1. **✅ Agregar Producto al Carrito**
2. **✅ Actualizar Cantidad**
3. **✅ Verificar Stock Insuficiente**
4. **✅ Eliminar Producto del Carrito**
5. **✅ Limpiar Carrito Completo**
6. **✅ Checkout Transaccional**
7. **✅ Verificar Actualización de Stock**

## 🔮 Futuras Implementaciones

### Para Activar Módulo de Pagos:

1. **Descomentar entidades** en `Project.Domain/Entities/Payment.cs`
2. **Descomentar repositorios** en `Project.Infrastructure/Repositories/PaymentRepository.cs`
3. **Descomentar servicios** en `Project.Application/Services/PaymentService.cs`
4. **Descomentar controlador** en `Api/Controllers/PaymentController.cs`
5. **Descomentar configuraciones** en EF
6. **Registrar servicios** en `Program.cs`
7. **Crear migración** para tablas de pagos

### Integraciones de Pago Preparadas:
- 💳 **Stripe**
- 🌐 **PayPal** 
- 🏦 **APIs Bancarias Locales**
- ⚡ **Procesadores de Criptomonedas**

## 🎯 Ventajas del Sistema Implementado

| Funcionalidad | Antes | Después |
|---------------|-------|---------|
| **Carrito** | ❌ No existía | ✅ Sistema completo |
| **Stock** | ❌ Manual | ✅ Automático |
| **Checkout** | ❌ Directo a factura | ✅ Desde carrito |
| **Pagos** | ❌ No preparado | ✅ Módulo listo |
| **UX** | ❌ Básica | ✅ Experiencia completa |

## 🏆 **Estado Actual: LISTO PARA PRODUCCIÓN**

✅ **Carrito de compras funcional**  
✅ **Checkout transaccional**  
✅ **Gestión de stock automática**  
✅ **Clean Architecture mantenida**  
✅ **Módulo de pagos preparado**  
✅ **PostgreSQL preparado**  
✅ **Documentación completa**  

## 👨‍💻 Autor

**Daniel-devlp**
- GitHub: [@daniel-devlp](https://github.com/daniel-devlp)

---

> 🛒 **Proyecto con sistema de carrito de compras completo** siguiendo Clean Architecture.
> 
> 💳 **Módulo de pagos preparado** para implementación futura.

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Consulta el archivo [LICENSE](LICENSE) para más detalles.
