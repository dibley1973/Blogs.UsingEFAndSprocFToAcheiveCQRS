using Blogs.EfAndSprocfForCqrs.DomainModel.Context;
using Blogs.EfAndSprocfForCqrs.DomainModel.Repositories;
using System;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Transactional
{
    internal class UnitOfWork : IDisposable
    {
        private readonly CommandContext _context;

        public UnitOfWork(CommandContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            _context = context;

            Orders = new OrderRepository(context);
        }

        private OrderRepository Orders { get; set; }

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
        }

        ~UnitOfWork()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }
    }
}