# Using Entity Framework and the Store Procedure Framework To Achieve CQRS - Part #3
This article follows on from Part 2 where we built the *QueryStack*. In this article we will build the *Command Stack* and we will leverage *EntityFramework* to do this.

So moving on to the "Blogs.EfAndSprocfForCqrs.DomainModel" project, as we intend to use the *Entity Framework* for data access in the *CommandStack* lets use *NuGet* to add the *Entity Framework* library to this project. You can use the *Package Manager* in *Visual Studio* or the console. I prefer to use the GUI in Visual studio but if you prefer to use the console then the command line is  below.

    PM> Install-Package EntityFramework
    
The next task is to set up the *DomainModel* folder structure in the project.

    |--04.CommandStack
    |  +--Blogs.EfAndSprocfForCqrs.DomainModel
    |     +--Context
    |     +--Dtos
    |     +--Entities
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


