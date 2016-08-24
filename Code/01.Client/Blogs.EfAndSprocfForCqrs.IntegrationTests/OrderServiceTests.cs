using Blogs.EfAndSprocfForCqrs.Services.Commands;
using Blogs.EfAndSprocfForCqrs.Services.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;

namespace Blogs.EfAndSprocfForCqrs.IntegrationTests
{
    [TestClass]
    public class OrderServiceTests
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateNewOrderForCustomerWithProducts_WhenGivenNullCommand_ThrowsException()
        {
            // ARRANGE
            var orderService = Dependencies.Defaults.DefaultOrderService;

            // ACT
            orderService.CreateNewOrderForCustomerWithProducts(null);

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateNewOrderForCustomerWithProducts_WhenGivenCommandWithEmptyCustomerId_ThrowsException()
        {
            // ARRANGE
            var productsOnOrder = new List<int> { 8, 9 };
            const string customerOrderNumber = "00123";
            var command = new CreateNewOrderForCustomerWithProductsCommand
            {
                CustomerId = Guid.Empty,
                ProductsOnOrder = productsOnOrder,
                CustomerOrderNumber = customerOrderNumber
            };
            var orderService = Dependencies.Defaults.DefaultOrderService;

            // ACT
            orderService.CreateNewOrderForCustomerWithProducts(command);

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateNewOrderForCustomerWithProducts_WhenGivenCommandWithEmptyOrderId_ThrowsException()
        {
            // ARRANGE
            var customerId = new Guid("17e3a22e-07e5-4ab2-8e62-1b15f9916909");
            const string customerOrderNumber = "00123";
            var command = new CreateNewOrderForCustomerWithProductsCommand
            {
                OrderId = Guid.Empty,
                CustomerId = customerId,
                ProductsOnOrder = null,
                CustomerOrderNumber = customerOrderNumber
            };
            var orderService = Dependencies.Defaults.DefaultOrderService;

            // ACT
            orderService.CreateNewOrderForCustomerWithProducts(command);

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateNewOrderForCustomerWithProducts_WhenGivenCommandWithNullProductsOnOrder_ThrowsException()
        {
            // ARRANGE
            var customerId = new Guid("17e3a22e-07e5-4ab2-8e62-1b15f9916909");
            const string customerOrderNumber = "00123";
            var command = new CreateNewOrderForCustomerWithProductsCommand
            {
                CustomerId = customerId,
                ProductsOnOrder = null,
                CustomerOrderNumber = customerOrderNumber
            };
            var orderService = Dependencies.Defaults.DefaultOrderService;

            // ACT
            orderService.CreateNewOrderForCustomerWithProducts(command);

            // ASSERT
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CreateNewOrderForCustomerWithProducts_WhenGivenInvalidProducts_ThrowsException()
        {
            // ARRANGE
            var customerId = new Guid("17e3a22e-07e5-4ab2-8e62-1b15f9916909");
            var productsOnOrder = new List<int> { 88, 99 };
            const string customerOrderNumber = "00123";
            var orderService = Dependencies.Defaults.DefaultOrderService;
            var orderId = orderService.CreateOrderId();
            var command = new CreateNewOrderForCustomerWithProductsCommand
            {
                OrderId = orderId,
                CustomerId = customerId,
                ProductsOnOrder = productsOnOrder,
                CustomerOrderNumber = customerOrderNumber
            };

            // ACT
            orderService.CreateNewOrderForCustomerWithProducts(command);
        }

        [TestMethod]
        public void CreateNewOrderForCustomerWithProducts_WhenGivenValidProducts_THEN()
        {
            // ARRANGE
            var customerId = new Guid("17e3a22e-07e5-4ab2-8e62-1b15f9916909");
            var productsOnOrder = new List<int> { 8, 9 };
            const string customerOrderNumber = "00123";
            var orderService = Dependencies.Defaults.DefaultOrderService;
            var orderId = orderService.CreateOrderId();
            var command = new CreateNewOrderForCustomerWithProductsCommand
            {
                OrderId = orderId,
                CustomerId = customerId,
                ProductsOnOrder = productsOnOrder,
                CustomerOrderNumber = customerOrderNumber
            };
            OrderDetailsModel actual;

            // ACT
            using (var transaction = new TransactionScope()) // This may not actually work??
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
    }
}