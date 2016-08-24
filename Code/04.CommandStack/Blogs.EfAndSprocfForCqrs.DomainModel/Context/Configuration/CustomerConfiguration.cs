using System.Data.Entity.ModelConfiguration;
using Blogs.EfAndSprocfForCqrs.DomainModel.Entities;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Context.Configuration
{
    public class CustomerConfiguration : EntityTypeConfiguration<Customer>
    {
        public CustomerConfiguration()
        {
            HasKey(customer => customer.Id);
        }
    }
}