using System;
using System.Collections.Generic;

namespace Blogs.EfAndSprocfForCqrs.ReadModel.Dtos
{
    public class OrderDetailsDto
    {
        public OrderDetailsDto()
        {
            ProductsOnOrder = new List<ProductsOrderedDto>();
        }

        public Guid Id { get; set; }
        public string CustomerOrderNumber { get; set; }
        public DateTime CreatedOnTimeStamp { get; set; }

        public CustomerDto OrderOwner { get; set; }
        public List<ProductsOrderedDto> ProductsOnOrder { get; private set; }
    }
}