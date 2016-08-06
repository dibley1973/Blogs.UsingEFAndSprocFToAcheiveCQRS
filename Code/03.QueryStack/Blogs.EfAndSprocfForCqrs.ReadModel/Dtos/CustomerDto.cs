using System;

namespace Blogs.EfAndSprocfForCqrs.ReadModel.Dtos
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime RegisteredDate { get; set; }
        public bool Active { get; set; }
    }
}