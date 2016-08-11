using System;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Entities
{
    internal class Product
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTimestamp { get; set; }
    }
}