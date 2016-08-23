using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Entities
{
    public class Order
    {
        private readonly List<ProductOnOrder> _productsOnOrder;

        public Order()
        {
            _productsOnOrder = new List<ProductOnOrder>();
        }

        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerOrderNumber { get; set; }
        public DateTime CreatedOnTimeStamp { get; set; }
        public virtual ReadOnlyCollection<ProductOnOrder> ProductsOnOrder {
            get { return _productsOnOrder.AsReadOnly(); }
        }
        public virtual Customer OrderOwner { get; set; }

        public void AddProductsOnOrder(List<ProductOnOrder> productsOnOrder)
        {
            if(productsOnOrder == null) throw new ArgumentNullException("productsOnOrder");
            if(productsOnOrder.Count == 0) return;

            _productsOnOrder.AddRange(productsOnOrder);
        }
    }
}