using System;
using System.Collections.Generic;

namespace Blogs.EfAndSprocfForCqrs.ReadModel.Dtos
{
    public class OrderDto
    {
        public OrderDto()
        {
            ProductsOrdered = new List<ProductsOrderedDto>();
        }

        public Guid Id { get; set; }
        public string CustomerOrderNumber { get; set; }
        public DateTime CreatedOnTimeStamp { get; set; }

        public CustomerDto Customer { get; set; }
        public List<ProductsOrderedDto> ProductsOrdered { get; set; }
    }
}