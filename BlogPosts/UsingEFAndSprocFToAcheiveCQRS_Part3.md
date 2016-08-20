# Using Entity Framework and the Store Procedure Framework To Achieve CQRS - Part #3
This article follows on from Part 2 where we built the *QueryStack* and queried an order and some of its associated data, in this article we will shift our focus to the *Command Stack* and we will leverage *EntityFramework* to do add a new order to the database.

So  moving on to the "Blogs.EfAndSprocfForCqrs.DomainModel" project, as we intend to use the *Entity Framework* for data access in the *CommandStack* lets use *NuGet* to add the *Entity Framework* library to this project. You can use the *Package Manager* in *Visual Studio* or the console. I prefer to use the GUI in Visual studio but if you prefer to use the console then the command line is  below.

    PM> Install-Package EntityFramework
    
The next task is to set up the *DomainModel* folder structure in the project.

    |--04.CommandStack
    |  +--Blogs.EfAndSprocfForCqrs.DomainModel
    |     +--Context
    |     +--Dtos
    |     +--Entities
    |     +--Factories
    |     +--Repositories
    |     +--Transactional
    |

Create the *Customer* Entity...
    
    internal class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime RegisteredDate { get; set; }
        public bool Active { get; set; }
    } 
   
Create the *Order* Entity...

    internal class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string CustomerOrderNumber { get; set; }
        public DateTime CreatedOnTimeStamp { get; set; }

        public Customer OrderOwner { get; set; }
    }

Create *Product* entity

    internal class Product
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedTimestamp { get; set; }
    }

Create *ProductOrdered* entity

    internal class ProductOrdered
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public decimal PurchasePrice { get; set; }
    }


create *CommandContext*...

    
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