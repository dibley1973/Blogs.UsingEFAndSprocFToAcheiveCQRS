using System;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Entities
{
    internal class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerOrderNumber { get; set; }
        public DateTime CreatedOnTimeStamp { get; set; }

        public Customer OrderOwner { get; set; }
    }
}