using System;
using System.Collections.Generic;
using Blogs.EfAndSprocfForCqrs.ReadModel.Dtos;

namespace Blogs.EfAndSprocfForCqrs.Services.Models
{
    public class OrderDetailsModel
    {
        public Guid Id { get; private set; }
        public string CustomerOrderNumber { get; private set; }
        public DateTime CreatedOnTimeStamp { get; private set; }
        public CustomerModel OrderOwner { get; private set; }
        public List<ProductModel> ProductsOnOrder { get; private set; }

        public OrderDetailsModel(OrderDetailsDto orderDetails)
        {
            if (orderDetails == null) throw new ArgumentNullException("orderDetails");

            ProductsOnOrder = new List<ProductModel>();

            CreatedOnTimeStamp = orderDetails.CreatedOnTimeStamp;
            CustomerOrderNumber = orderDetails.CustomerOrderNumber;
            OrderOwner = new CustomerModel(orderDetails.OrderOwner);
            Id = orderDetails.Id;
            LoadProductsOnOrder(orderDetails.ProductsOnOrder);
        }

        private void LoadProductsOnOrder(List<ProductsOrderedDto> productsOnOrder)
        {
            foreach (var productsOrderedDto in productsOnOrder)
            {
                var productOnOrder = new ProductModel
                {
                    Description = productsOrderedDto.Description,
                    Id = productsOrderedDto.Id,
                    Key = productsOrderedDto.Key,
                    Name = productsOrderedDto.Name,
                    ProductId = productsOrderedDto.ProductId,
                    PurchasePrice = productsOrderedDto.PurchasePrice
                };

                ProductsOnOrder.Add(productOnOrder);
            }
        }
    }
}