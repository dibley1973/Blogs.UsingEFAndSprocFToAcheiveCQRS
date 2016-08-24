
using Blogs.EfAndSprocfForCqrs.DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Factories
{
    public static class OrderFactory
    {
        public static List<ProductOnOrder> CreateProductsOnOrder(Guid orderId, List<Product> productsOnOrder)
        {
            if (productsOnOrder == null) throw new ArgumentNullException("productsOnOrder");

            var result = new List<ProductOnOrder>();
            if (!productsOnOrder.Any()) return result;

            foreach (var product in productsOnOrder)
            {
                var productOnOrder = new ProductOnOrder
                {
                    ProductId = product.Id,
                    OrderId = orderId,
                    PurchasePrice = product.Price
                };
                result.Add(productOnOrder);
            }

            return result;
        }

        public static Guid CreateNewOrderId()
        {
            return new Guid();
        }

        public static Order CreateOrderFrom(Guid orderId, Guid customerId, string customerOrderNo, List<ProductOnOrder> productsOnOrder)
        {
            var order = new Order();
            order.Id = orderId;
            order.CustomerId = customerId;
            order.CustomerOrderNumber = customerOrderNo;
            order.CreatedOnTimeStamp = DateTime.Now;
            order.AddProductsToOrder(productsOnOrder);
            return order;
        }
    }
}