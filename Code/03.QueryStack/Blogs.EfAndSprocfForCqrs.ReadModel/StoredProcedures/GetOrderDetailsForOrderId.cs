using Blogs.EfAndSprocfForCqrs.ReadModel.Dtos;
using Dibware.StoredProcedureFramework.Base;
using System;
using System.Collections.Generic;

namespace Blogs.EfAndSprocfForCqrs.ReadModel.StoredProcedures
{
    internal class GetOrderDetailsForOrderId
        : StoredProcedureBase<GetOrderDetailsForOrderId.ResultSet, GetOrderDetailsForOrderId.Parameter>
    {
        public GetOrderDetailsForOrderId(GetOrderDetailsForOrderId.Parameter parameters)
            : base(parameters)
        { }

        internal class Parameter
        {
            public Guid Id { get; set; }
        }

        internal class ResultSet
        {
            public ResultSet()
            {
                Orders = new List<OrderDto>();
                Customers = new List<CustomerDto>();
                ProductsOrdered = new List<ProductsOrderedDto>();
            }

            public List<OrderDto> Orders { get; set; }
            public List<CustomerDto> Customers { get; set; }
            public List<ProductsOrderedDto> ProductsOrdered { get; set; }
        }
    }
}