using Blogs.EfAndSprocfForCqrs.DomainModel.Context;
using Blogs.EfAndSprocfForCqrs.DomainModel.Transactional;
using Blogs.EfAndSprocfForCqrs.ReadModel.Context;
using Blogs.EfAndSprocfForCqrs.ReadModel.ReadModels;
using Blogs.EfAndSprocfForCqrs.Services;

namespace Blogs.EfAndSprocfForCqrs.Dependencies
{
    public static class Defaults
    {
        private readonly static string DefaultConnectionString = Properties.Settings.Default.DefaultConnection;

        private static ReadContext DefaultContext
        {
            get
            {
                return new ReadContext(DefaultConnectionString);
            }
        }

        private static OrderReadModel DefaultOrderReadModel
        {
            get
            {
                return new OrderReadModel(DefaultContext);
            }
        }

        public static OrderService DefaultOrderService
        {
            get
            {
                return new OrderService(DefaultOrderReadModel, DefaultUnitOfWork);
            }
        }

        private static UnitOfWork DefaultUnitOfWork
        {
            get
            {
                return new UnitOfWork(DefaultCommandContext);
            }
        }

        private static CommandContext DefaultCommandContext
        {
            get
            {
                return new CommandContext(DefaultConnectionString);
            }
        }
    }
}