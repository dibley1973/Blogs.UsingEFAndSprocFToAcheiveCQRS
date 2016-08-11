
using System;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Entities
{
    internal class ProductOrdered
    {
        public int Id { get; set; }
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal PurchasePrice { get; set; }
    }
}