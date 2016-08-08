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

So we have a class to handle our database connection, and next we need a class to represent the "GetOrderDetailsForOrderId" stored procedure which we will be calling to get our order, and it's closely associated data. It may seem a bit of an overkill as the ReadContext does little more than pass through to the SqlConnection it holds. I have chosen this approach for one key reason, when we do the command model there will be a reflection in the objects created, so it will better help analysis of teh similarities and differences between using EF and SprocF.

In addition it is my intention a future release of the *Stored Procedure Framework* to add a base context class which will be similar to EntityFramework's DBConext, so the API will be similar to how a developer uses EF. At present this will require a breaking change so will not likely be implemented in the *Stored Procedure Framework* until version 2.0 where it will be more acceptable to introduce a breaking change to the API.

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


We also need a constructor which tales the parameters and passes them on to the base class.

        public GetOrderDetailsForOrderId(GetOrderDetailsForOrderId.Parameter parameters)
            :base(parameters)
        { }

The finished stored procedure class should look like below. 

    internal class GetOrderDetailsForOrderId
        : StoredProcedureBase<GetOrderDetailsForOrderId.ResultSet, GetOrderDetailsForOrderId.Parameter>
    {
        public GetOrderDetailsForOrderId(GetOrderDetailsForOrderId.Parameter parameters)
            :base(parameters)
        {
            
        }
        
        internal class Parameter
        {
            public Guid OrderId { get; set; }
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
  
 
Next in the ReadModels folder create a new public class called *OrderReadModel* which will use the "GetOrderDetailsForOrderId" class along with the *Stored Procedure Framework* to pull data from the database and load the DTOs with data. We know we will return an *OrderDetailsDto* do we could just make this the return type like so...

    public OrderDetailsDto GetOrderDetails(Guid id)
    {
        throw new NotImplementedException();
    }
 
This method signature indicates that it will return an instance of an *OrderDetailsDto*. However if an id for a order that does not exist in the database is passed then the method result will be NULL. This results in any calling code having to ask if the result is NULL before acting upon the result. I'm not a big fan of this so what I propose is we return a wrapper object that will either contain a result or not, but the wrapper object itself will ALWAYS be an instance. So what we need is a wrapper that indicates ONE or ZERO results were found. for this we can use the following generic class.

    public class SingleSearchResult<T> : IEnumerable<T>
    {
        private readonly T _result;

        public SingleSearchResult()
        {
            _result = default(T);
        }

        public SingleSearchResult(T result)
        {
            if (result == null) throw new ArgumentNullException("result");

            _result = result;
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (ResultWasFound)
            {
                yield return _result;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets a value indicating whether a result was found.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a result was found; otherwise, <c>false</c>.
        /// </value>
        public bool ResultWasFound
        {
            get { return _result != null; }
        }

        /// <summary>
        /// Gets the result (providing one was found); otherwise and exception is thrown.
        /// </summary>
        /// <value>
        /// The result.
        /// </value>
        /// <exception cref="System.InvalidOperationException">
        /// No result was present. Please use 'ResultWasFound' property 
        /// to check for result before calling the 'Result' function.
        /// </exception>
        public T Result
        {
            get
            {
                if (ResultWasFound) return _result;

                throw new InvalidOperationException(
                    "No result was present. Please use 'ResultWasFound' property " + 
                    "to check for result before calling the 'Result' function. ");
            }
        }
    }

The class expects a type parameter, which will identify the type we want the *SingleSearchResult* to hold. The class can either be constructed with a single instance of this type or with nothing. If with an instance was passed in then the *ResultWasFound* property will return true and the *Result* function can be called to retrieve the result. If the parameterless constructor was called then the *ResultWasFound* property will return false, and if the *Result* function called an exception will be thrown. The class also implements the *IEnumerable<T>* interface so can be enumerated through once if a result was found.

Now we have the *SingleSearchResult* we can change our method signature to indicate to the caller what the results of the method call will be in a clearer manner.

        public SingleSearchResult<OrderDetailsDto> GetOrderDetails(Guid id)
        {
            throw new NotImplementedException();
        }

Before we create the implementation we need to pass in the ReadContext into the ReadModel.

    public class OrderReadModel
    {
        private readonly ReadContext _readContext;

        public OrderReadModel(ReadContext readContext)
        {
            Guard.ArgumentIsNotNull(readContext, "readContext");

            _readContext = readContext;
        }

Now we have a way to access the database, lets get on and create the implementation. First we need to create an instance of the stored procedures parameters and then the stored procedure its self. We can then call the procedure through the ReadContext and hold onto the results.

        public SingleSearchResult<OrderDetailsDto> GetOrderDetails(Guid id)
        {
            var parameters = new GetOrderDetailsForOrderId.Parameter { OrderId = id };
            var procedure = new GetOrderDetailsForOrderId(parameters);

            GetOrderDetailsForOrderId.ResultSet procedureResult = _readContext.Connection.ExecuteStoredProcedure(procedure);

If we did not return an order then we need to just return an empty *SingleSearchResult*.

            if (!procedureResult.Orders.Any()) return new SingleSearchResult<OrderDetailsDto>();

Otherwise we can get on and process the results. We get the first order and use that to populate the properties of our *OrderDetailsDto*. We then add the customer as the *OrderOwner* and finally the products which were on the order.

            var firstOrder = procedureResult.Orders.First();
            var result = new OrderDetailsDto
            {
                CreatedOnTimeStamp = firstOrder.CreatedOnTimeStamp,
                CustomerOrderNumber = firstOrder.CustomerOrderNumber,
                Id = firstOrder.Id,
                OrderOwner =  procedureResult.Customers.FirstOrDefault()
            };
            result.ProductsOnOrder.AddRange(procedureResult.ProductsOrdered);

And then return a *SingleSearchResult* with a *OrderDetailsDto* contained within it.

            return new SingleSearchResult<OrderDetailsDto>(result);

We are now ready to call this code, so lets go to the services project *Blogs.EfAndSprocfForCqrs.Services* and add a class called *OrderService*. This service will provide all of the orchestration for the client between the ReadModel and WriteModel giving a single "endpoint" (*I use this term loosely*) for the client to connect to. The *Blogs.EfAndSprocfForCqrs.Services*  will need a reference to the *Blogs.EfAndSprocfForCqrs.ReadModel* project and then we can add field to hold a reference to the *OrderReadModel* in the *OrderService* class.

    public class OrderService
    {
        private readonly OrderReadModel _orderReadModel;

        public OrderService(OrderReadModel orderReadModel)
        {
            if (orderReadModel == null) throw new ArgumentNullException("orderReadModel");

            _orderReadModel = orderReadModel;
        }
    }

Lets now create a function to return the order details to the client. The function will return an *OrderDetailsModel* (which we have not yet defined) and will query the OrderReadModel to get the data for this. Once a response has returned from the *OrderReadModel* we will check if a result was found and if it was not we will throw an exception. This may unnecessarily seem "harsh" but in theory this method should never be called without a valid Order Id. If it is then it is either due to a bug in the calling code or an attempt to access data that should not. If we have a result then we can go ahead and construct the *OrderDetailsModel* from the *OrderDetailsDto*. We have chosen not to just expose the *OrderDetailsDto*  straight to the client for two reasons. The first is to do this we would either have to let the client have a reference to the *Blogs.EfAndSprocfForCqrs.ReadModel* so it can see the *OrderDetailsDto* or we would have to declare the *OrderDetailsDto* in a *shared* project that multiple layers can see, which is not something I am adverse to but just choose not to in this case due to the second reason. The second reason is that this service method may also do some additional orchestration and data gathering which may need to be appended to the object being returned. If we use the DTO then the DTO has to have extra properties which were not needed to be populated by the OrderReadmodel in the first call. It may seem like unnecessary duplication of code, but I feel each layer and each class in each layer should only be interested in what *it* needs to know. So Lets populate define and populate the *OrderDetialsModel*

    public class OrderDetailsModel
    {
        public Guid Id { get; private set; }
        public string CustomerOrderNumber { get; private set; }
        public DateTime CreatedOnTimeStamp { get; private set; }
        public CustomerModel OrderOwner { get; private set; }
        public List<ProductModel> ProductsOnOrder { get; private set; }

        public OrderDetailsModel(OrderDetailsDto orderDetails)
        {
            if (orderDetails == null) throw new ArgumentNullException("orderDetails");

            ProductsOnOrder = new List<ProductModel>();

            CreatedOnTimeStamp = orderDetails.CreatedOnTimeStamp;
            CustomerOrderNumber = orderDetails.CustomerOrderNumber;
            OrderOwner = new CustomerModel(orderDetails.OrderOwner);
            Id = orderDetails.Id;
            LoadProductsOnOrder(orderDetails.ProductsOnOrder);
        }

        private void LoadProductsOnOrder(List<ProductsOrderedDto> productsOnOrder)
        {
            foreach (var productsOrderedDto in productsOnOrder)
            {
                var productOnOrder = new ProductModel
                {
                    Description = productsOrderedDto.Description,
                    Id = productsOrderedDto.Id,
                    Key = productsOrderedDto.Key,
                    Name = productsOrderedDto.Name,
                    ProductId = productsOrderedDto.ProductId,
                    PurchasePrice = productsOrderedDto.PurchasePrice
                };

                ProductsOnOrder.Add(productOnOrder);
            }
        }
    }

    public class CustomerModel
    {
        public CustomerModel(CustomerDto orderOwner)
        {
            Id = orderOwner.Id;
            Active = orderOwner.Active;
            Name = orderOwner.Name;
            RegisteredDate = orderOwner.RegisteredDate;
        }

        public Guid Id { get; private set; }
        public bool Active { get; private set; }
        public string Name { get; private set; }
        public DateTime RegisteredDate { get; private set; }
    }
    
    public class ProductModel
    {
        public string Description { get; set; }
        public int Id { get; set; }
        public string Key { get; set; }
        public string Name { get; set; }
        public int ProductId { get; set; }
        public decimal PurchasePrice { get; set; }
    }

That's it for the OrderService, now lets move to our Client...., well.. the *Blogs.EfAndSprocfForCqrs.IntegrationTests* project! We will start by adding adding a reference to the *Blogs.EfAndSprocfForCqrs.Services* project, adding a *ReadModelTests* class which will represent our calling client and within this creating a new test to represent a call to the service with an invalid OrderId. Within this test we will instantiate a new instance of the *OrderService*... but wait... we can't do this as the constructor requires an instance of an *OrderReadModel* which we just don't have a reference for! Damn. So normally I'd use a dependency injection framework like NInject to handle this, but for the sake of brevity lets just set a quick project called *Blogs.EfAndSprocfForCqrs.Dependencies* in the *01.Client* folder to handle all of our default dependencies. This project will need project references to *Blogs.EfAndSprocfForCqrs.ReadModel* and *Blogs.EfAndSprocfForCqrs.Services* and will contain a single static class.

    public static class Defaults
    {
        public readonly static string DefaultConnectionString = Properties.Settings.Default.DefaultConnection;

        public static ReadContext DefaultContext
        {
            get
            {
                return new ReadContext(DefaultConnectionString);
            }
        }

        public static OrderReadModel DefaultOrderReadModel
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
                return new OrderService(DefaultOrderReadModel);
            }
        }
    }

So NOW we can finally move to our "client" and create a test for retrieving data for an order with an ID that does not exists. Because one does not exist we would expect an InvalidOperationException to be raised.

    [TestClass]
    public class ReadModelTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void GetOrderForId_ThrowsException_WhenGivenInvalidId()
        {
            // ARRANGE
            var orderService = Dependencies.Defaults.DefaultOrderService;
            var invalidId = new Guid("0bab4fc6-d749-455c-afee-73cfb0a01d08");

            // ACT
            orderService.GetOrderForId(invalidId);
        }
    }
    
Now lets add a test to simulate retrieving order details for an order whose ID does exist.

    [TestMethod]
    public void GetOrderForId_ReturnsOrderDetails_WhenGivenAValidId()
    {
        // ARRANGE
        var orderService = Dependencies.Defaults.DefaultOrderService;
        var invalidId = new Guid("4a61a22a-bade-d780-bbfa-be19c7746d87");

        // ACT
        var model = orderService.GetOrderForId(invalidId);

        // ASSERT
        Assert.AreEqual("17e3a22e-07e5-4ab2-8e62-1b15f9916909", model.OrderOwner.Id.ToString());
        Assert.AreEqual("0000001", model.CustomerOrderNumber);
        Assert.AreEqual("2016/01/02 11:08:34", model.CreatedOnTimeStamp.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture));
        Assert.IsTrue(model.OrderOwner.Active);
        Assert.AreEqual("17e3a22e-07e5-4ab2-8e62-1b15f9916909", model.OrderOwner.Id.ToString());
        Assert.AreEqual("Mike Finnegan", model.OrderOwner.Name);
        Assert.AreEqual("1961/01/19 00:00:00", model.OrderOwner.RegisteredDate.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture));
        Assert.AreEqual(3, model.ProductsOnOrder.Count);
        Assert.AreEqual(1, model.ProductsOnOrder.First().Id);
        Assert.AreEqual(5, model.ProductsOnOrder.First().ProductId);
        Assert.AreEqual("snapon-ratchet-ring-metric-10-21", model.ProductsOnOrder.First().Key);
        Assert.AreEqual("Snap-On Metric Ratchet Ring Set 10-21mm", model.ProductsOnOrder.First().Name);
    }

So now we have called our service for an order that does not exist and for one that does. This about wraps up this article. In the next article we will look at using Entity Framework for the *Command Stack*. If you wish to reread the part one, the link is below.

### Links

[Using Entity Framework and the Store Procedure Framework To Achieve CQRS - Part 1] (https://github.com/dibley1973/Blogs.UsingEFAndSprocFToAcheiveCQRS/blob/master/BlogPosts/UsingEFAndSprocFToAcheiveCQRS_Part1.md)

This post and all corresponding code and data can be found on my GitHub project - [https://github.com/dibley1973/Blogs.UsingEFAndSprocFToAcheiveCQRS](https://github.com/dibley1973/Blogs.UsingEFAndSprocFToAcheiveCQRS)

### Disclaimer

I am the author of the *Stored Procedure Framework*. 