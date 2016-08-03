using System;
using Dibware.StoredProcedureFramework.Base;
using System.Collections.Generic;
using Blogs.EfAndSprocfForCqrs.ReadModel.Dtos;

namespace Blogs.EfAndSprocfForCqrs.ReadModel.StoredProcedures
{
    internal class GetOrderDetailsForOrderId
        : StoredProcedureBase<GetOrderDetailsForOrderId.ResultSet, GetOrderDetailsForOrderId.Parameter>
    {
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