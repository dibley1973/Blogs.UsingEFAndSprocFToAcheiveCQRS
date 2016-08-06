# Using Entity Framework and the Store Procedure Framework To Achieve CQRS - Part #2

This article follows on from part 1 where we described the background to the problem, set up the solution and added seed data to the database. In this article we will tackle the *QueryStack* and more specifically the *ReadModel*.

So moving on to the "Blogs.EfAndSprocfForCqrs.ReadModel" project, as we intend to use the *Stored Procedure Framework* for data access in the *QueryStack* lets use *NuGet* to add the *Stored Procedure Framework* library to this project. At the time of writing version 1.0.3. is available on NuGet so this is the version I will add to the project. You can use the Package Manager in visual studio or the console. I prefer to use the GUI in Visual studio but if you prefer to use the console then the command line is  below.

    PM> Install-Package Dibware.StoredProcedureFramework
    
The next task is to set up the *ReadModel* folder structure in the project.

|--03.QueryStack
|  +--Blogs.EfAndSprocfForCqrs.ReadModel
|     +--Context
|     +--Dtos
|     +--ReadModels
|     +--StoredProcedures
|

In the *Context* folder create a new public class called *ReadContext*. Give it a private field of type *SqlConnection* and make it implement the *Dispose Pattern*, closing and disposing of the connection within The disposing path of the *Dispose(bool disposing)* method. This will ensure that when our *ReadContext* is disposed our connection is closed and cleaned up.

    public class ReadContext : IDisposable
    {
        private bool _disposed;

        private SqlConnection _connection;

        public ReadContext(string connectionString)
        {
            Guard.ArgumentIsNotNullOrEmpty(connectionString, "connectionString");

            _connection = new SqlConnection(connectionString);
        }

        internal SqlConnection Connection
        {
            get { return _connection; }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        ~ReadContext()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing) CloseAndDisposeConnection();
            

            _disposed = true;
        }

        private void CloseAndDisposeConnection()
        {
            if (_connection == null) return;

            if (_connection.State != ConnectionState.Closed) _connection.Close();

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }
    }

So we have a class to handle our database connection, and next we need a class to represent the "GetOrderDetailsForOrderId" stored procedure which we will be calling to get our order, and it's closely associated data.

In the *StoredProcedures* folder create a new internally scoped class called *GetOrderDetailsForOrderId* which inherits from *StoredProcedureBase<TReturn, TParameter>* with TReturn being a generic *List* of *OrderDto* objects and with the TParamet being of type *GetOrderDetailsForOrderId.Parameter*, both yet to be defined. 

    internal class GetOrderDetailsForOrderId
        : StoredProcedureBase<GetOrderDetailsForOrderId.ResultSet, GetOrderDetailsForOrderId.Parameter>
    {
    }

Create a nested *Parameter* class within the stored procedure class with a single property called "Id" of type *System.Guid* and with public getter and setter accessors.
 
        internal class Parameter
        {
            public Guid Id { get; set; }
        }

Create a nested *ResultSet* class within the stored procedure class with three public properties which are lists of Dtos which represent the three RecordSets that will be returned from the stored procedure. Ensure these properties are instantiated in the class's constructor.

        internal class ResultSet
        {
            public ResultSet()
            {
                Orders = new List<OrderDto>();
                Customers = new List<CustomerDto>();
                ProductsOrdered = new List<ProductsOrderedDto>();
            }
            
            public List<OrderDto> Orders { get; set; }
            public List<CustomerDto> Customers { get; set; }
            public List<ProductsOrderedDto> ProductsOrdered { get; set; }
        }

The finished stored procedure class should look like below. 

    internal class GetOrderDetailsForOrderId
        : StoredProcedureBase<GetOrderDetailsForOrderId.ResultSet, GetOrderDetailsForOrderId.Parameter>
    {
        internal class Parameter
        {
            public Guid Id { get; set; }
        }

        internal class ResultSet
        {
            public List<OrderDto> Orders { get; set; }
            public List<CustomerDto> Customers { get; set; }
            public List<ProductsOrderedDto> ProductsOrdered { get; set; }
        }
    }
 
Now we need some public DTO classes too represent the rows being returned from the stored procedure. Lets start with the *OrderDto* which represents the rows from the first Recordset in the stored procedure.

    public class OrderDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerOrderNumber { get; set; }
        public DateTime CreatedOnTimeStamp { get; set; }
    }
    
The second Recordset contains the customer information so lets create a *CustomerDto* DTO to represent that.
    
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime RegisteredDate { get; set; }
        public bool Active { get; set; }
    }

And lastly we need a DTO to represent the products on the order, the *ProductsOrderedDto*.

    public class ProductsOrderedDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal PurchasePrice { get; set; }
    }

So these DTOs will represents the rows returned from the stored procedure, but it would be nice to wrap these into a single object to return to the client. For this we will have an *OrderDetailsDto*. This will include some of the properties of the *OrderDto* and will have an "Id", a "CustomerOrderNumber" and a "CreatedOnTimeStamp" property. It will also encapsulate the *CustomerDto* and contain a list of *ProductsOrdered*. 

    public class OrderDetailsDto
    {
        public OrderDetailsDto()
        {
            ProductsOnOrder = new List<ProductsOrderedDto>();
        }

        public Guid Id { get; set; }
        public string CustomerOrderNumber { get; set; }
        public DateTime CreatedOnTimeStamp { get; set; }

        public CustomerDto OrderOwner { get; set; }
        public List<ProductsOrderedDto> ProductsOnOrder { get; private set; }
    }
  
 
Next in the ReadModels folder create a new public class called *OrderReadModel* which will use the "GetOrderDetailsForOrderId" class along with the *Stored Procedure Framework* to pull data from the database and load the DTOs with data.

 
 

