using System;
using System.Collections.Generic;

namespace Blogs.EfAndSprocfForCqrs.Services.Commands
{
    public class CreateNewOrderForCustomerWithProductsCommand
    {
        public Guid CustomerId { get; set; }
        public Dictionary<int, int> ProductsOnOrder { get; set; }
    }
}
