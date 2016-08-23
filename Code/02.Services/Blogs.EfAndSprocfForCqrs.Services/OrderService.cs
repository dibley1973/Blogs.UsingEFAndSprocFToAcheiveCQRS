using Blogs.EfAndSprocfForCqrs.DomainModel.Transactional;
using Blogs.EfAndSprocfForCqrs.ReadModel.ReadModels;
using Blogs.EfAndSprocfForCqrs.Services.Commands;
using Blogs.EfAndSprocfForCqrs.Services.Models;
using System;
using System.Collections.Generic;
using Blogs.EfAndSprocfForCqrs.DomainModel.Entities;
using Blogs.EfAndSprocfForCqrs.DomainModel.Factories;

namespace Blogs.EfAndSprocfForCqrs.Services
{
    public class OrderService
    {
        private readonly OrderReadModel _orderReadModel;
        private readonly UnitOfWork _unitOfWork;

        public OrderService(OrderReadModel orderReadModel, UnitOfWork unitOfWork)
        {
            if (orderReadModel == null) throw new ArgumentNullException("orderReadModel");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            _orderReadModel = orderReadModel;
            _unitOfWork = unitOfWork;
        }

        public OrderDetailsModel GetOrderForId(Guid id)
        {
            var query = _orderReadModel.GetOrderDetails(id);
            var resultNotFound = query.ResultWasFound == false;

            if (resultNotFound) throw new InvalidOperationException("The requested order was not found. ");

            OrderDetailsModel model = new OrderDetailsModel(query.Result);

            return model;
        }

        /// <exception cref="System.ArgumentNullException">command</exception>
        /// <exception cref="System.ArgumentException">
        /// command.CustomerId
        /// or
        /// command.ProductsOnOrder
        /// </exception>
        public void CreateNewOrderForCustomerWithProducts(CreateNewOrderForCustomerWithProductsCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (command.CustomerId == Guid.Empty) throw new ArgumentException("command.CustomerId");
            if (command.ProductsOnOrder == null) throw new ArgumentException("command.ProductsOnOrder");

            var orderId = OrderFactory.CreateNewOrderId();

            List<ProductOnOrder> productsOnOrder = OrderFactory.CreateProductsOnOrder(orderId, command.ProductsOnOrder);
            Order order = OrderFactory.CreateOrderFrom(orderId, command.CustomerId, productsOnOrder);

            //_unitOfWork.Orders.Add(order);
            //_unitOfWork.Complete();
        }
    }
}