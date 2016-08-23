using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerOrderNumber { get; set; }
        public DateTime CreatedOnTimeStamp { get; set; }
        public virtual ReadOnlyCollection<ProductOnOrder> ProductsOnOrder { get; set; }
        public virtual Customer OrderOwner { get; set; }

        public void AddProductsOnOrder(List<ProductOnOrder> productsOnOrder)
        {
            throw new NotImplementedException();
        }
    }
}