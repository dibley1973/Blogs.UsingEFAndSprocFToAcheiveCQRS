using Blogs.EfAndSprocfForCqrs.DomainModel.Context;
using Blogs.EfAndSprocfForCqrs.DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Repositories
{
    public class ProductRepository
    {
        private readonly CommandContext _context;

        public ProductRepository(CommandContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            _context = context;
        }

        public IEnumerable<Product> GetProductsForIds(List<int> idList)
        {
            return _context.Set<Product>()
                .Where(product => idList.Contains(product.Id));
        }
    }
}
