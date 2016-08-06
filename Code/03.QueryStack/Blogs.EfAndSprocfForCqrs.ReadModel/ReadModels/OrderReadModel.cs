using Blogs.EfAndSprocfForCqrs.ReadModel.Context;
using Blogs.EfAndSprocfForCqrs.ReadModel.Dtos;
using Blogs.EfAndSprocfForCqrs.ReadModel.QueryResults;
using Blogs.EfAndSprocfForCqrs.ReadModel.StoredProcedures;
using Dibware.Helpers.Validation;
using Dibware.StoredProcedureFramework.Extensions;
using System;
using System.Linq;

namespace Blogs.EfAndSprocfForCqrs.ReadModel.ReadModels
{
    public class OrderReadModel
    {
        private readonly ReadContext _readContext;

        public OrderReadModel(ReadContext readContext)
        {
            Guard.ArgumentIsNotNull(readContext, "readContext");

            _readContext = readContext;
        }

        public SingleSearchResult<OrderDetailsDto> GetOrderDetails(Guid id)
        {
            var parameters = new GetOrderDetailsForOrderId.Parameter { Id = id };
            var procedure = new GetOrderDetailsForOrderId(parameters);

            GetOrderDetailsForOrderId.ResultSet procedureResult = _readContext.Connection.ExecuteStoredProcedure(procedure);

            if (!procedureResult.Orders.Any()) return new SingleSearchResult<OrderDetailsDto>();

            var firstOrder = procedureResult.Orders.First();
            var result = new OrderDetailsDto
            {
                CreatedOnTimeStamp = firstOrder.CreatedOnTimeStamp,
                CustomerOrderNumber = firstOrder.CustomerOrderNumber,
                Id = firstOrder.Id,
                OrderOwner =  procedureResult.Customers.FirstOrDefault()
            };
            result.ProductsOnOrder.AddRange(procedureResult.ProductsOrdered);

            return new SingleSearchResult<OrderDetailsDto>(result);
        }
    }
}