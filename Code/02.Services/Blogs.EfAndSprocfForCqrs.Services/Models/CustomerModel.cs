using System;
using Blogs.EfAndSprocfForCqrs.ReadModel.Dtos;

namespace Blogs.EfAndSprocfForCqrs.Services.Models
{
    public class CustomerModel
    {
        public CustomerModel(CustomerDto orderOwner)
        {
            Id = orderOwner.Id;
            Active = orderOwner.Active;
            Name = orderOwner.Name;
            RegisteredDate = orderOwner.RegisteredDate;
        }

        public Guid Id { get; private set; }
        public bool Active { get; private set; }
        public string Name { get; private set; }
        public DateTime RegisteredDate { get; private set; }
    }
}