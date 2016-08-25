# Using Entity Framework and the Store Procedure Framework To Achieve CQRS - Part #3

This article follows on from [Part 3]() where we built the *CommandStack* leveraging *EntityFramework* and prepared to persist an order and some of its associated data to the database. In this article we will shift our focus to the the client, (well our Integration Tests!) and create a test that creates an order, persists it using the *CommandStack* and reads it back out with the *QueryStack*. 

Since part 2 I have renamed *ReadModelTests* to be *OrderServiceTests* as that now better represents what we are testing. We do need to make a tweak to the "dependency injection" mocker, the *Defaults* class before we can add any tests. Please bear in mind the *Defaults* class is just to *new-up* some of our objects. Normally i would use the *Ninject* DI framework for this purpose.  


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
                return new OrderService(DefaultOrderReadModel, DefaultUnitOfWork);
            }
        }

        public static UnitOfWork DefaultUnitOfWork
        {
            get
            {
                return new UnitOfWork(DefaultConnectionString);
            }
        }
    }

I'm not going to go into each of the tests I have written (however they are all in the source code on [GitHub](https://github.com/dibley1973/Blogs.UsingEFAndSprocFToAcheiveCQRS)). We will only focus on one test that will show off our *Command Stack*. We wil create an command for the new order for an existing customer, and two of the products in the database. We will use the service to generated a new order Id and then create the *CreateNewOrderForCustomerWithProductsCommand*. (IDEA: This command maybe better named as `CreateNewOrderWithProductsForCustomerCommand`?). Once the command is created we will use a new `TransactionScope` so we can roll back the transaction and leave the database in the state we found it after each run of the test. Once inside the transaction we will use the OrderService to create the order. We will then use the OrderReadModel (from the second post in this series) to read the new order details back out so we may check all of the order details are correct once we have rolled back the transaction.

        [TestMethod]
        public void CreateNewOrderForCustomerWithProducts_WhenGivenValidProducts_CreatesAnOrder()
        {
            // ARRANGE
            var customerId = new Guid("17e3a22e-07e5-4ab2-8e62-1b15f9916909");
            var productsOnOrder = new List<int> { 8, 9 };
            const string customerOrderNumber = "00123";
            var orderService = Defaults.DefaultOrderService;
            var orderId = orderService.GenerateNewOrderId();
            var command = new CreateNewOrderForCustomerWithProductsCommand
            {
                OrderId = orderId,
                CustomerId = customerId,
                ProductsOnOrder = productsOnOrder,
                CustomerOrderNumber = customerOrderNumber
            };
            OrderDetailsModel actual;

            // ACT
            using (var transaction = new TransactionScope()) 
            {
                orderService.CreateNewOrderForCustomerWithProducts(command);

                actual = orderService.GetOrderForId(orderId);

                transaction.Dispose();
            }

            // ASSERT
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.ProductsOnOrder);
            Assert.AreEqual(2, actual.ProductsOnOrder.Count);

            var actualProduct1 = actual.ProductsOnOrder.FirstOrDefault(p => p.ProductId == 8);
            Assert.IsNotNull(actualProduct1);
            Assert.AreEqual(14.99M, actualProduct1.PurchasePrice);

            var actualProduct2 = actual.ProductsOnOrder.FirstOrDefault(p => p.ProductId == 9);
            Assert.IsNotNull(actualProduct2);
            Assert.AreEqual(29.99M, actualProduct2.PurchasePrice);
        }

## Summary

So there we have it. A small solution showing how you can leverage the *StoredProcedureFramework* and *EntityFramework* to acheive CQRS in a .Net application. All of the code is available in [this GitHub]() reporsitory.


[Part 1]() Setting up the data
[Part 2]() Querying the database using the QueryStack
[Part 3]() Writing to the datbasea using the CommandStack