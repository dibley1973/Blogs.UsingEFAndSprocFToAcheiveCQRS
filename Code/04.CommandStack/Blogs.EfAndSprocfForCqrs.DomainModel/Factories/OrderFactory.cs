
using Blogs.EfAndSprocfForCqrs.DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Factories
{
    public static class OrderFactory
    {
        public static Guid CreateNewOrderId()
        {
            return Guid.NewGuid();
        }

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

        public static Order CreateOrderFrom(Guid orderId, Guid customerId, string customerOrderNo, List<ProductOnOrder> productsOnOrder)
        {
            if (orderId == Guid.Empty) throw new ArgumentOutOfRangeException("orderId", "Order Id must not be empty");
            if (customerId == Guid.Empty) throw new ArgumentOutOfRangeException("customerId", "Customer Id must not be empty");
            if (productsOnOrder == null) throw new ArgumentNullException("productsOnOrder");
            if (!productsOnOrder.Any()) throw new ArgumentOutOfRangeException("productsOnOrder", "An order must have products on it. ");

            var order = new Order
            {
                Id = orderId,
                CustomerId = customerId,
                CustomerOrderNumber = customerOrderNo,
                CreatedOnTimeStamp = DateTime.Now
            };
            order.AddProductsToOrder(productsOnOrder);
            return order;
        }
    }
}