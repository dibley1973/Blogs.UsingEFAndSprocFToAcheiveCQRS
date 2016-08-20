
using System;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Entities
{
    public class ProductOnOrder
    {
        public int Id { get; set; }
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal PurchasePrice { get; set; }
    }
}