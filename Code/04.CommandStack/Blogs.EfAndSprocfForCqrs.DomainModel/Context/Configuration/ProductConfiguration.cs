﻿using Blogs.EfAndSprocfForCqrs.DomainModel.Entities;
using System.Data.Entity.ModelConfiguration;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Context.Configuration
{
    public class ProductConfiguration : EntityTypeConfiguration<Product>
    {
        public ProductConfiguration()
        {
            HasKey(product => product.Id);
        }
    }
}