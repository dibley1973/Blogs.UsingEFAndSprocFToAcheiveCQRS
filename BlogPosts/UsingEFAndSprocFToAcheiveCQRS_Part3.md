# Using Entity Framework and the Store Procedure Framework To Achieve CQRS - Part #3
This article follows on from [Part 2]() where we built the *QueryStack* and queried an order and some of its associated data, in this article we will shift our focus to the *Command Stack* and we will leverage *EntityFramework* to do add a new order to the database.

Before we can get started we have to make a small amendment to the *dbo.Product* table structure. I had omitted the *Price* column. 

CREATE TABLE [dbo].[Product] (
    [Id]               INT            IDENTITY (1, 1) NOT NULL,
    [Key]              NVARCHAR (128) NOT NULL,
    [Name]             NVARCHAR (255) NOT NULL,
    [Description]      NVARCHAR (MAX) NOT NULL,
    [CreatedTimestamp] DATETIME       NOT NULL,
    [Price] MONEY NOT NULL, 
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([Id] ASC)
);

We will also need to re-seed this table too with the price data added.

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
	VALUES (9, N'pry-master-prybar', N'Pri-Master 24" Pry-Bar', N'Pri-Master 24" Pry-Bar with plastic ergonomic handle', N'2015-11-26 17:00:02', 29.99)
INSERT INTO [dbo].[Product] ([Id], [Key], [Name], [Description], [CreatedTimestamp], [Price]) 
	VALUES (10, N'snap-on-6-inch-slip-pliers', N'Snap-On 6 Inch Slip Pliers', N'Snap-On 6 Inch Slip Pliers, ideal for removing brake spring clips', N'2015-12-02 09:33:22', 45.99)
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

Now to create the *Order* Entity. This entity also has some basic properties. It also has a *ReadOnlyCollection* of products which are on the order. It is backed by a private list which can only be added to with the *AddProductsToOrder* method. We also have a property of type *Customer* which is the owner of the order and our navigation property, and along side this (for convenience) we will also hold the Customer Id as a foreign key property. IF this seems a bit awkward / or an overkill see great Pluralsight course on using EntityFramework by *Julie Lerman*. In the course she highlights why having the foreign key makes your life easier even if it does "muddy" your entity model a little. [This link](https://msdn.microsoft.com/en-gb/data/hh134698.aspx) also mentiones the benefits. Anyway, its down to personal preference, I put them in, you can choose not to.

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

The *Product* entity is straight forward.

    public class Product
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public decimal Price { get; set; }
    }

As is the *ProductOnOrder* entity, although this does have a navigation property which is a reference to the parent *Order* object.

    public class ProductOnOrder
    {
        public int Id { get; set; }
        public Guid OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal PurchasePrice { get; set; }

        public virtual Order Order { get; set; }
    }

Now we need to quickly flip back into the *Configuration* folder inside the *Context* folder to add in all of the *EntityTypeConfiguration* classes. The *CustomerConfiguration* just defines the primary key.

    public class CustomerConfiguration : EntityTypeConfiguration<Customer>
    {
        public CustomerConfiguration()
        {
            HasKey(customer => customer.Id);
        }
    }

The *OrderConfiguration* defines the primary key and also the one to many Customer to Order relationship using the fluent API.

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

The *ProductConfiguration* is simple again with just the primary key defined.

    public class ProductConfiguration : EntityTypeConfiguration<Product>
    {
        public ProductConfiguration()
        {
            HasKey(product => product.Id);
        }
    }

And lastly the *ProductOnOrderConfiguration* has the primary key and the one to many relationship for the Order to the products on the order. 

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

Lets now move to the *Factories* folder and add an *OrderFactory* class whose purpose will be to construct an order for us. When we create a new order we will need a unique identifier for it. For simplicity in this article I have chosen to use Guids so we will need a method in the factory that will return a new *Guid*. I know that using non-sequential Guids in a large database table can cause a performance concern so some people tend to shy away from them. Some people may prefer to use a *Long Integer* with a *high-low* strategy for generation, but for a web application where the Order ID may be passed in a query string or as a route parameter a non-guessable ID seems a more preferable idea for customer and order identifiers to me. So for the purpose of this article, Guids it is! 

    public static class OrderFactory
    {
        public static Guid CreateNewOrderId()
        {
            return Guid.NewGuid();
        }


We also need a way of building a list of *ProductOnOrder* which the order will carry for the order from a list of *Product* objects which the customer selected. The method basically iterates through the list of products creating new *ProductOnOrder* objects with them and the specified order id. 

public static List<ProductOnOrder> CreateProductsOnOrder(Guid orderId, List<Product> productsOnOrder)
        {
            if (productsOnOrder == null) throw new ArgumentNullException("productsOnOrder");

            var result = new List<ProductOnOrder>();
            if (!productsOnOrder.Any()) return result;

            foreach (var product in productsOnOrder)
            {
                var productOnOrder = new ProductOnOrder
                {
                    ProductId = product.Id,
                    OrderId = orderId,
                    PurchasePrice = product.Price
                };
                result.Add(productOnOrder);
            }

            return result;
        }

The last method in the factory creates an actual *Order* object it self, using an order id, customer id, a customer order number if supplied and of course a list of products to add ot the order.

        public static Order CreateOrderFrom(Guid orderId, Guid customerId, string customerOrderNo, List<ProductOnOrder> productsOnOrder)
        {
            if (orderId == Guid.Empty) throw new ArgumentOutOfRangeException("orderId", "Order Id must not be empty");
            if (customerId == Guid.Empty) throw new ArgumentOutOfRangeException("customerId", "Customer Id must not be empty");
            if (productsOnOrder == null) throw new ArgumentNullException("productsOnOrder");
            if (!productsOnOrder.Any()) throw new ArgumentOutOfRangeException("productsOnOrder", "An order must have products on it. ");

            var order = new Order
            {
                Id = orderId,
                CustomerId = customerId,
                CustomerOrderNumber = customerOrderNo,
                CreatedOnTimeStamp = DateTime.Now
            };
            order.AddProductsToOrder(productsOnOrder);
            return order;
        }

Ideally I should create a suite of unit tests for the functions in the *OrderFactory*, but it is not the scope of this article to go into the pros and cons of unit testing. 

Next up we need to focus on the repositories, so lets open the *Repositories* folder and lets start by creating a repository for the Order, the *OrderRepository*. The repository for the orders is relatively simple and straight forward. We could have methods like `Order Get(Guid id)` or `IEnumerable<Order> GetAllForCustomer(Guid customerId)` within the repository but for the purposes of this article we just need an `void Add(Order order)` method. The repository must first of all be constructed with our *CommandContext*.

    public class OrderRepository
    {
        private readonly CommandContext _context;

        public OrderRepository(CommandContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            _context = context;
        }

        public void Add(Order order)
        {
            if (order == null) throw new ArgumentNullException("order");

            _context.Set<Order>().Add(order);
        }
    }

We will also need a repository for the products as when we create an order we need some additional product information, for example the price at the time of ordering. So the *ProductRepository* will have a single method `IEnumerable<Product> GetProductsForIds(List<int> idList)` and like the order repository it will be constructed with our *CommandContext*.

    public class ProductRepository
    {
        private readonly CommandContext _context;

        public ProductRepository(CommandContext context)
        {
            if (context == null) throw new ArgumentNullException("context");

            _context = context;
        }

        public IEnumerable<Product> GetProductsForIds(List<int> idList)
        {
            return _context.Set<Product>()
                .Where(product => idList.Contains(product.Id));
        }
    }

As we will be updating two tables in the database when we create an order, one to hold the order and one to hold the products on the order we need to ensure that our updating is carried out in a single atomic action. For this we will use the UnitOfWork pattern. So lets move to the *Transactional* folder and create our *UnitOfWork*. As the unit of work will hold it's own instance of the CommandContext and will need to ensure it disposes of this correctly it will need to implement the *IDisposable* interface.

    public class UnitOfWork : IDisposable
    {
        private bool _disposed;
        private readonly CommandContext _context;

We will condtruct the *UnitOfWork* with a connection name or connection string, and use this to instantiate a *CommandContext*. This will live around for the life of the UnitOfWork and be disposed along with the UnitOfWork by following the Dispose Pattern. The UnitOfWork will contin two repositories, an *OrderRepository* and a *ProductsRepository*. These will both be instantiated with the CommandContext insstance.

        public UnitOfWork(string nameOrConnectionString)
        {
            if (string.IsNullOrWhiteSpace(nameOrConnectionString)) throw new ArgumentNullException("nameOrConnectionString");

            _context = new CommandContext(nameOrConnectionString);

            Orders = new OrderRepository(_context);
            Products = new ProductRepository(_context);
        }

As well as the properties to expose the repositories there is a single method `Complete()` which will call `SaveChanges()` on the *CommandContext*. This method is called when all updates made to the repositories need to be persisted back to the database.

        /// <summary>
        /// Called to complete a unit of work.
        /// </summary>
        public void Complete()
        {
            _context.SaveChanges();
        }

Now the *UnitOfWork* is complete, we can focus on the *Blogs.EfAndSprocfForCqrs.Services* project and open the *OrderService* and use what we have created so far in this post to add a new order to the database. First we will add another private field to the service and this will hold a reference to the *UnitOfWork*. we will initialize it from the constructor just like we do with the OrderReadModel. we will pass the *UnitOfWork* into the service as we may want many services to perform actions all in one atomic transaction. for this reason we will not dispose of the *UnitOfWork* when the service dies, we will let the constructing code handle disposing of the *UnitOfWork* for us.

        private readonly OrderReadModel _orderReadModel;
        private readonly UnitOfWork _unitOfWork;

        public OrderService(OrderReadModel orderReadModel, UnitOfWork unitOfWork)
        {
            if (orderReadModel == null) throw new ArgumentNullException("orderReadModel");
            if (unitOfWork == null) throw new ArgumentNullException("unitOfWork");

            _orderReadModel = orderReadModel;
            _unitOfWork = unitOfWork;
        }

Now we need to add a new method to the service to create the new order for the customer along with the products they have ordered. We will use a command object to carry the information we need from the client to create the order. So lets add a *Commands* folder in the *Services* project and within it create a *CreateNewOrderForCustomerWithProductsCommand* command. We actually don't need much data from the client, all we need is the *OrderId*, the *CustomerId*, the customer's order number, and a list  containing the product IDs for the products on order. (In a real world scenario we'd probably want to hold the quantities of the product as well, but if I am honest, I simply forgot!)

    public class CreateNewOrderForCustomerWithProductsCommand
    {
        public Guid OrderId { get; set; }
        public Guid CustomerId { get; set; }
        public List<int> ProductsOnOrder { get; set; }
        public string CustomerOrderNumber { get; set; }
    }

Now we have a command to carry out order information, we can create the service method to create the order passing the command as the Parameter. The method will have a few guard clauses to try and ensure we cannot pass a command with an invalid state to the method. We then use the list of product Ids to get a list of products from the database. We check if we got less products back than the ids passed in and throw an exception if we do.

Providing we got an equal quantity of products back we will use the order factory to create a list of *ProductOnOrder* items. We will then pass this along with the Order Id, Customer Id and Customer Order Number to the OrderFactory to create an new *Order*. This will be added to the *OrderRepository* via the *UnitOfWork* and then we will complete the transaction.

        /// <exception cref="System.ArgumentNullException">command</exception>
        /// <exception cref="System.ArgumentException">
        /// command.CustomerId
        /// or
        /// command.ProductsOnOrder
        /// </exception>
        public void CreateNewOrderForCustomerWithProducts(CreateNewOrderForCustomerWithProductsCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");
            if (command.OrderId == Guid.Empty) throw new ArgumentOutOfRangeException("command.OrderId", "OrderId must not be empty");
            if (command.CustomerId == Guid.Empty) throw new ArgumentOutOfRangeException("command.CustomerId", "CustomerId must not be empty");
            if (command.ProductsOnOrder == null) throw new ArgumentException("command.ProductsOnOrder");
            if (command.ProductsOnOrder.Count == 0) throw new ArgumentOutOfRangeException("command.ProductsOnOrder", "ProductsOnOrder must not be empty");

            var productsOrdered = _unitOfWork.Products.GetProductsForIds(command.ProductsOnOrder).ToList();
            var productCountShortfall = command.ProductsOnOrder.Count - productsOrdered.Count;
            if (productCountShortfall > 0) throw new InvalidOperationException(productCountShortfall + " products on order not found! ");

            var productsOnOrder = OrderFactory.CreateProductsOnOrder(command.OrderId, productsOrdered);
            var order = OrderFactory.CreateOrderFrom(command.OrderId, command.CustomerId, command.CustomerOrderNumber, productsOnOrder);

            _unitOfWork.Orders.Add(order);
            _unitOfWork.Complete();
        }

While we are in the service we will provide a method for generating new valid OrderIds. In our case we will just let the *OrderFactory* create a new *Guid*, but we may have had to go to the database to get the next sequential Guid, or the next available *Long Integer* using a *High-Low* strategy. 

In Part 4 we move to the client, (well our Integration Tests!) and 

[Part 1]() Setting up the data
[Part 2]() Querying the database using the QueryStack

### Disclaimer

I am the author of the Stored Procedure Framework.