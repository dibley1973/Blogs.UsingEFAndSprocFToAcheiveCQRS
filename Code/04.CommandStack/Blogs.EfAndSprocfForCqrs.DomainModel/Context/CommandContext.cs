using Blogs.EfAndSprocfForCqrs.DomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;

namespace Blogs.EfAndSprocfForCqrs.DomainModel.Context
{
    public class CommandContext : DbContext
    {
        #region Constructors

        // Uncomment to use Entity Framework Power Tools EDMX viewer
        //public CommandContext() {}

        public CommandContext(string connectionOrName)
            : base(connectionOrName)
        {
            Configuration.LazyLoadingEnabled = false;

            DisableDatabaseInitializer();
        }

        #endregion

        #region DBSets

        public DbSet<Order> Order { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<Customer> Customer { get; set; }

        #endregion

        #region DbContext Member Overrides

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            RemoveConventions(modelBuilder);
            AddAllEntityConfigurations(modelBuilder);
            //AddEntityConfigurations(modelBuilder);
        }

        #endregion

        private static void DisableDatabaseInitializer()
        {
            Database.SetInitializer<CommandContext>(null);
        }

        /// <summary>
        /// Adds all entity configurations.
        /// </summary>
        /// <remarks>
        /// Courtesy of: octavioccl on Stack overflow, here: http://stackoverflow.com/a/27748465/254215
        /// </remarks>
        private void AddAllEntityConfigurations(DbModelBuilder modelBuilder)
        {
            var configurationsToRegister = GetAllEntityConfigurationsToRegister();

            RegisterEntityTypeConfigurations(modelBuilder, configurationsToRegister);

            base.OnModelCreating(modelBuilder);
        }

        private static void RegisterEntityTypeConfigurations(DbModelBuilder modelBuilder, IEnumerable<Type> configurationsToRegister)
        {
            foreach (var type in configurationsToRegister)
            {
                dynamic configurationInstance = Activator.CreateInstance(type);
                modelBuilder.Configurations.Add(configurationInstance);
            }
        }

        private static IEnumerable<Type> GetAllEntityConfigurationsToRegister()
        {
            var entityConfigurationsToRegister = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => !String.IsNullOrEmpty(type.Namespace))
                .Where(type => type.BaseType != null
                               && type.BaseType.IsGenericType
                               && type.BaseType.GetGenericTypeDefinition() == typeof(EntityTypeConfiguration<>));

            return entityConfigurationsToRegister;
        }

        //private static void AddEntityConfigurations(DbModelBuilder modelBuilder)
        //{
        //    modelBuilder.Configurations.Add(new OrderConfiguration());
        //    modelBuilder.Configurations.Add(new ProductConfiguration());
        //    modelBuilder.Configurations.Add(new ProductOnOrderConfiguration());
        //}

        private static void RemoveConventions(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
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