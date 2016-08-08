using Blogs.EfAndSprocfForCqrs.ReadModel.ReadModels;
using System;
using Blogs.EfAndSprocfForCqrs.Services.Models;

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
}