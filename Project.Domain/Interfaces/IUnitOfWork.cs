namespace Project.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IClientRepository Clients { get; }
        IProductRepository Products { get; }
        IInvoiceRepository Invoices { get; }
        IShoppingCartRepository ShoppingCart { get; }
        
        // Interfaces para módulo de pagos (comentadas para implementación futura)
        /*
        IPaymentRepository Payments { get; }
        IPaymentMethodRepository PaymentMethods { get; }
        */
      
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}