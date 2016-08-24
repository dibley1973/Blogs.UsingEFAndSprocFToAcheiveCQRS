# Using Entity Framework and the Store Procedure Framework To Achieve CQRS - Part #3
This article follows on from Part 2 where we built the *QueryStack* and queried an order and some of its associated data, in this article we will shift our focus to the *Command Stack* and we will leverage *EntityFramework* to do add a new order to the database.

Before we can get started we have to make a small amendment to the *dbo.Product* table structure. I had ommited the *Price* column. 

CREATE TABLE [dbo].[Product] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [Key]              NVARCHAR (128) NOT NULL,
    [Name]             NVARCHAR (255) NOT NULL,
    [Description]      NVARCHAR (MAX) NOT NULL,
    [CreatedTimestamp] DATETIME       NOT NULL,
    [Price] MONEY NOT NULL, 
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([Id] ASC)
);

We will also need to ressed this table too.

SET IDENTITY_INSERT [dbo].[Product] ON
INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp], [Price]) 
	VALUES (2, N'compressor-mac-2hp', N'2HP MAC Tools Compressor', N'Super dandy 2HP  portable compressor by MAC Tools ', N'2015-11-23 16:00:23', 450.00)
INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp], [Price]) 
	VALUES (5, N'snapon-ratchet-ring-metric-10-21', N'Snap-On Metric Ratchet Ring Set 10-21mm', N'Snap-On Metric Ratchet set 10mm - 21mm', N'2015-11-24 00:00:00', 180.00)
INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp], [Price]) 
	VALUES (7, N'snap-on-ratchet-screwdriver-red', N'Snap-On Ratchet Screwdriver in Red', N'Snap-On Ratchet Screwdriver in Red with six bits included', N'2015-11-24 01:01:24', 85.00)
INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp], [Price]) 
	VALUES (8, N'usmash-4lb-hammer', N'U-Smash 4lb lump hammer', N'U-Smash 4lb lump hammer', N'2015-11-25 16:00:35', 14.99)
INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp], [Price]) 
	VALUES (9, N'pry-master-prybar', N'Pri-Master 24" Pry-Bar', N'Pri-Master 24" Pry-Bar with plastic ergonmoic handle', N'2015-11-26 17:00:02', 29.99)
INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp], [Price]) 
	VALUES (10, N'snap-on-6-inch-slip-plyers', N'Snap-On 6 Inch Slip Plyers', N'Snap-On 6 Inch Slip Plyers, ideal for removing brake spring clips', N'2015-12-02 09:33:22', 45.99)
SET IDENTITY_INSERT [dbo].[Product] OFF

So now that is out of the way we can move on to the "Blogs.EfAndSprocfForCqrs.DomainModel" project, as we intend to use the *Entity Framework* for data access in the *CommandStack* lets use *NuGet* to add the *Entity Framework* library to this project. You can use the *Package Manager* in *Visual Studio* or the console. I prefer to use the GUI in Visual studio but if you prefer to use the console then the command line is  below.

    PM> Install-Package EntityFramework
    
The next task is to set up the *DomainModel* folder structure in the project.

    |--04.CommandStack
    |  +--Blogs.EfAndSprocfForCqrs.DomainModel
    |     +--Context
    |     |  +--Configuration
    |     +--Entities
    |     +--Factories
    |     +--Repositories
    |     +--Transactional
    |


Lets start in the *Context* folder and create the *CommandContext*. This will Inherit from EntityFramework's *DbContext*. It will have a constructor that will take a connection name or connection string and it will pass this on to the base class. We will switch off lazy loading as we don't need to worry about performance for this article. We will also disable the database initializer as we already have a perfectly good database to use.

    public class CommandContext : DbContext
    {
        // Uncomment parameterless constructor if you want to use the 'Entity Framework Power Tools' EDMX viewer
        //public CommandContext() {}

        public CommandContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Configuration.LazyLoadingEnabled = false;

            DisableDatabaseInitializer();
        }

        private static void DisableDatabaseInitializer()
        {
            Database.SetInitializer<CommandContext>(null);
        }

We will need four DbSets of entities, *Customer*,*Order*,*Product* and *ProductOnOrder*. We haven't created the entities yet, but will get on to them in a short while.

        public DbSet<Customer> Customer { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductOnOrder> ProductOnOrder { get; set; }

We need to override the *OnModelCreating* method to remove the pluralising convention and to apply all *EntityTypeConfiguration* classes to the *DbModelbuilder*. I used to manually instantiate every EntityTypeConfiguration class and this would nearly always catch me out as I'd forget to plumb new ones in. However courtesy of an answer by *octavioccl* on stack overflow I now have a more generic "catch-all" version which scans the assembly for all *EntityTypeConfiguration*.

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            RemoveConventions(modelBuilder);
            AddAllEntityConfigurations(modelBuilder);
        }

        private static void RemoveConventions(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
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

The next step is to head over to the *Entities* folder and create the actual entities we need. First we will create the *Customer* Entity. As well as some basic properties we also have list of orders which is accessed by a ReadOnlyCollection. We are not doing much with this collection for this article, but we do need the collection for our EF configuration later.

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

Now to create the *Order* Entity. This entity also has some basic properties. It also has a *ReadOnlyCollection* of products which are on the order. It is backed by a private list which can only be added to with the *AddProductsToOrder* method.

    public class Order
    {
        private readonly List<ProductOnOrder> _productsOnOrder;

        public Order()
        {
            _productsOnOrder = new List<ProductOnOrder>();
        }

        public Guid Id { get; set; }
        public string CustomerOrderNumber { get; set; }
        public DateTime CreatedOnTimeStamp { get; set; }
        public virtual ReadOnlyCollection<ProductOnOrder> ProductsOnOrder {
            get { return _productsOnOrder.AsReadOnly(); }
        }

        public Guid CustomerId { get; set; }
        public virtual Customer OrderOwner { get; set; }

        public void AddProductsToOrder(List<ProductOnOrder> productsOnOrder)
        {
            if(productsOnOrder == null) throw new ArgumentNullException("productsOnOrder");
            if(productsOnOrder.Count == 0) return;

            _productsOnOrder.AddRange(productsOnOrder);
        }
    }








Create *Product* entity

    public class Product
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public decimal Price { get; set; }
    }

Create *ProductOrdered* entity

    internal class ProductOrdered
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal PurchasePrice { get; set; }
    }




    
So lets start by creating a repository for the Order, the *OrderRepository* .

    internal class OrderRepository
    {
        private readonly CommandContext _context;

        public OrderRepository(CommandContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            _context = context;
        }

        public void Add(Order order)
        {
            _context.Set<Order>().Add(order);
        }

        public Order Get(Guid id)
        {
            return _context.Set<Order>().Find(id);
        }

        public IEnumerable<Order> GetAll()
        {
            return _context.Set<Order>().ToList();
        }
    }





Now we can look at creating a unit of work.


Lets now focus on the *Blogs.EfAndSprocfForCqrs.Services* project and open the *OrderService* tackle how we use what we have created so far in this post to add a new order to the database.

First add a private field to hold a reference to the *UnitOfWork* and intialize it from the constructor like the OrderReadModel. 

        private readonly UnitOfWork _unitOfWork;

        public OrderService(OrderReadModel orderReadModel, UnitOfWork unitOfWork)
        {
            if (orderReadModel == null) throw new ArgumentNullException("orderReadModel");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            _orderReadModel = orderReadModel;
            _unitOfWork = unitOfWork;
        }

Now we need to add a method to create the new order for the customer along with the products they have ordered. We will use a command object to carry the information we need from the client to create the order. So lets add a *Commands* folder in ther *Services* project and within it create a *CreateNewOrderForCustomerWithProductsCommand* command. We actually don't need much data from the client, all we need is the *CustomerId* and a dictionary containing the product IDs and their quantities.

    public class CreateNewOrderForCustomerWithProductsCommand
    {
        public Guid CustomerId { get; set; }
        public Dictionary<int, int> ProductQuantities { get; set; }
    }

Now we can create the service method using the command as the Parameter.

        public void CreateNewOrderForCustomerWithProducts(CreateNewOrderForCustomerWithProductsCommand command)
        {
        }


dont forget to alter and add in Tthe UnitOfWork and DefaultCommandContext...
        public static OrderService DefaultOrderService
        {
            get
            {
                return new OrderService(DefaultOrderReadModel, DefaultUnitOfWork);
            }
        }


In the client (Integration Tests) renamed *ReadModelTests* to be *OrderServiceTests*