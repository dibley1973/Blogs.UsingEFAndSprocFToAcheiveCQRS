using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Entities
{
    public class Customer
    {
        private readonly List<Order> _orders;

        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime RegisteredDate { get; set; }
        public bool Active { get; set; }

        public virtual ReadOnlyCollection<Order> Orders
        {
            get { return _orders.AsReadOnly(); }
        }
    }
}