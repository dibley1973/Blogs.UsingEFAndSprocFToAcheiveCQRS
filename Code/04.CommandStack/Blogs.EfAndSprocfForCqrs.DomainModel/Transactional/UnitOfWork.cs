using Blogs.EfAndSprocfForCqrs.DomainModel.Context;
using Blogs.EfAndSprocfForCqrs.DomainModel.Repositories;
using System;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Transactional
{
    public class UnitOfWork : IDisposable
    {
        private bool _disposed;
        private readonly CommandContext _context;

        //public UnitOfWork(CommandContext context)
        //{
        //    if (context == null) throw new ArgumentNullException("context");

        //    _context = context;

        //    Orders = new OrderRepository(_context);
        //    Products = new ProductRepository(_context);
        //}

        public UnitOfWork(string nameOrConnectionString)
        {
            if (string.IsNullOrWhiteSpace(nameOrConnectionString)) throw new ArgumentNullException("nameOrConnectionString");

            _context = new CommandContext(nameOrConnectionString);

            Orders = new OrderRepository(_context);
            Products = new ProductRepository(_context);
        }

        public OrderRepository Orders { get; private set; }
        public ProductRepository Products { get; private set; }

        /// <summary>
        /// Called to complete a unit of work.
        /// </summary>
        public void Complete()
        {
            _context.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~UnitOfWork()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing) _context.Dispose();

            _disposed = true;
        }
    }
}