using Blogs.EfAndSprocfForCqrs.DomainModel.Entities;
using System.Data.Entity.ModelConfiguration;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Context.Configuration
{
    public class OrderConfiguration : EntityTypeConfiguration<Order>
    {
        public OrderConfiguration()
        {
            HasKey(order => order.Id);
            HasRequired(order => order.OrderOwner)
                .WithMany(customer => customer.Orders)
                .HasForeignKey(order => order.CustomerId);
        }
    }
}