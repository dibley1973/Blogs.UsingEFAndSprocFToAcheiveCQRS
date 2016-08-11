using System;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Entities
{
    internal class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime RegisteredDate { get; set; }
        public bool Active { get; set; }
    }
}