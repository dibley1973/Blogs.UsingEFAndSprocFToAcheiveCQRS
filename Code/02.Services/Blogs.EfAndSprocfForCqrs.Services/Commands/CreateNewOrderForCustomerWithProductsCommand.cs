using System;
using System.Collections.Generic;

namespace Blogs.EfAndSprocfForCqrs.Services.Commands
{
    public class CreateNewOrderForCustomerWithProductsCommand
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public List<int> ProductsOnOrder { get; set; }
        public string CustomerOrderNumber { get; set; }
    }
}