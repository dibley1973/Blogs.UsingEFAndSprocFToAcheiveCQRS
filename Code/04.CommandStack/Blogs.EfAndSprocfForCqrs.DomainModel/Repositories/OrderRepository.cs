using Blogs.EfAndSprocfForCqrs.DomainModel.Context;
using Blogs.EfAndSprocfForCqrs.DomainModel.Entities;
using System;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Repositories
{
    public class OrderRepository
    {
        private readonly CommandContext _context;

        public OrderRepository(CommandContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            _context = context;
        }

        public void Add(Order order)
        {
            if (order == null) throw new ArgumentNullException("order");

            _context.Set<Order>().Add(order);
        }

        //public Order Get(Guid id)
        //{
        //    return _context.Set<Order>().Find(id);
        //}

        //public IEnumerable<Order> GetAllForCustomer(Guid customerId)
        //{
        //    return _context.Set<Order>().Where(order => order.CustomerId == customerId).ToList();
        //}
    }
}