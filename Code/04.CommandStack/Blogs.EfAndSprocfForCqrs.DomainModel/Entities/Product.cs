using System;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public decimal Price { get; set; }
    }
}