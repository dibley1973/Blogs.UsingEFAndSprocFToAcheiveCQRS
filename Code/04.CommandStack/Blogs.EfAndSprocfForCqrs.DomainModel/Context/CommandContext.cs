using System;
using System.Data.Entity;
using Blogs.EfAndSprocfForCqrs.DomainModel.Entities;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Context
{
    internal class CommandContext : DbContext , IDisposable
    {
        public CommandContext()
        {
            Configuration.LazyLoadingEnabled = false;
        }

        //public virtual DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //modelBuilder.Configurations.Add(new CourseConfiguration());
        }

        //private bool _disposed;

        //public void Dispose()
        //{
        //    Dispose(true);
        //}

        //~CommandContext()
        //{
        //    Dispose(false);
        //    GC.SuppressFinalize(this);
        //}

        //private void Dispose(bool disposing)
        //{
        //    if (_disposed) return;

        //    //if (disposing) CloseAndDisposeConnection();
            
        //    base.Dispose(disposing);

        //    _disposed = true;
        //}

    }
}