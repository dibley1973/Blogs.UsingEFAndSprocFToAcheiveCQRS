using Blogs.EfAndSprocfForCqrs.DomainModel.Entities;
using System.Data.Entity.ModelConfiguration;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Context.Configuration
{
    public class ProductOnOrderConfiguration : EntityTypeConfiguration<ProductOnOrder>
    {
        public ProductOnOrderConfiguration()
        {
            ToTable("ProductOrdered");
            HasKey(productOnOrder => productOnOrder.Id);
            HasRequired(productOnOrder => productOnOrder.Order)
                .WithMany(order => order.ProductsOnOrder)
                .HasForeignKey(productOnOrder => productOnOrder.OrderId);
        }
    }
}