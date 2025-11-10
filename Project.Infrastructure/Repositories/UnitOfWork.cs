using Microsoft.EntityFrameworkCore.Storage;
using Project.Domain.Interfaces;
using Project.Infrastructure.Frameworks.EntityFramework;

namespace Project.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDBContext _context;
        private IDbContextTransaction _transaction;

        public UnitOfWork(ApplicationDBContext context)
        {
            _context = context;
        }

        public IClientRepository Clients => 
            _clientRepository ??= new ClientRepository(_context);
        private IClientRepository _clientRepository;

        public IProductRepository Products =>
            _productRepository ??= new ProductRepository(_context);
        private IProductRepository _productRepository;

        public IInvoiceRepository Invoices =>
            _invoiceRepository ??= new InvoiceRepository(_context);
        private IInvoiceRepository _invoiceRepository;

        public IShoppingCartRepository ShoppingCart =>
            _shoppingCartRepository ??= new ShoppingCartRepository(_context);
        private IShoppingCartRepository _shoppingCartRepository;

        // Repositorios para módulo de pagos (comentados para implementación futura)
        /*
        public IPaymentRepository Payments =>
         _paymentRepository ??= new PaymentRepository(_context);
        private IPaymentRepository _paymentRepository;

        public IPaymentMethodRepository PaymentMethods =>
            _paymentMethodRepository ??= new PaymentMethodRepository(_context);
        private IPaymentMethodRepository _paymentMethodRepository;
  */

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context?.Dispose();
        }
    }
}