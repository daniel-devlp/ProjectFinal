namespace Project.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IClientRepository Clients { get; }
        IProductRepository Products { get; }
        IInvoiceRepository Invoices { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}