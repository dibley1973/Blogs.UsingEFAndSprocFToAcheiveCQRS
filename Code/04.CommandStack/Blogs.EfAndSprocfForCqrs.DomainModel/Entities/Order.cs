using System;
using System.Collections.Generic;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerOrderNumber { get; set; }
        public DateTime CreatedOnTimeStamp { get; set; }
        public virtual List<ProductOnOrder> ProductsOnOrder { get; set; }
        public virtual Customer OrderOwner { get; set; }
    }
}