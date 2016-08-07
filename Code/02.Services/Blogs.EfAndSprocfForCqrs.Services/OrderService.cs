using Blogs.EfAndSprocfForCqrs.ReadModel.Dtos;
using Blogs.EfAndSprocfForCqrs.ReadModel.ReadModels;
using System;
using System.Collections.Generic;

namespace Blogs.EfAndSprocfForCqrs.Services
{
    public class OrderService
    {
        private readonly OrderReadModel _orderReadModel;

        public OrderService(OrderReadModel orderReadModel)
        {
            if (orderReadModel == null) throw new ArgumentNullException("orderReadModel");

            _orderReadModel = orderReadModel;
        }

        public OrderDetailsModel GetOrderForId(Guid id)
        {
            var query = _orderReadModel.GetOrderDetails(id);
            var resultNotFound = query.ResultWasFound == false;

            if (resultNotFound) throw new InvalidOperationException("The requested order was not found. ");

            OrderDetailsModel model = new OrderDetailsModel(query.Result);

            return model;
        }
    }

    public class OrderDetailsModel
    {
        public Guid Id { get; set; }
        public string CustomerOrderNumber { get; set; }
        public DateTime CreatedOnTimeStamp { get; set; }
        public CustomerModel OrderOwner { get; set; }
        public List<ProductModel> ProductsOnOrder { get; set; }

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
                
            }
        }
    }

    public class ProductModel
    {
    }

    public class CustomerModel
    {
        public CustomerModel(CustomerDto orderOwner)
        {
            Id = orderOwner.Id;
            Active = orderOwner.Active;
            Name = orderOwner.Name;
            RegisteredDate = orderOwner.RegisteredDate;
        }

        public Guid Id { get; set; }
        public bool Active { get; set; }
        public string Name { get; set; }
        public DateTime RegisteredDate { get; set; }
    }
}