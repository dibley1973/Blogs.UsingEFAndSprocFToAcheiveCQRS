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

        private readonly SqlConnection _connection;

        public ReadContext(string connectionString)
        {
            Guard.ArgumentIsNotNullOrEmpty(connectionString, "connectionString");

            _connection = new SqlConnection(connectionString);
        }

        internal SqlConnection Connection {
            get { return _connection;}
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
            if (!_disposed)
            {
                if (disposing)
                {
                    CloseAndDisposeConnection();
                }

                _disposed = true;
            }
        }

        private void CloseAndDisposeConnection()
        {
            if (_connection != null)
            {
                if (_connection.State != ConnectionState.Closed) _connection.Close();

                if (_connection != null)
                {
                    _connection.Dispose();
                }
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
 
Now we need some public Dto classes too represent the rows being returned from the stored procedure. Lets start with the *OrderDto*.

This will have "Id", "CustomerOrderNumber" and "CreatedOnTimeStamp" properties which relate to the database fields and will also have a property to hold a reference to a *CustomerDto*, and a list of *ProductsOrdered*. 

    public class OrderDto
    {
        public Guid Id { get; set; }
        public string CustomerOrderNumber { get; set; }
        public DateTime CreatedOnTimeStamp { get; set; }

        public CustomerDto Customer { get; set; }
        public List<ProductsOrderedDto> ProductsOrdered { get; set; } 
    }

 
The *CustomerDto* will have
 
 
Next in the ReadModels folder create a new public class called *OrderReadModel* which will use the "" class along with the *Stored Procedure Framework* to pull data from the database. 

