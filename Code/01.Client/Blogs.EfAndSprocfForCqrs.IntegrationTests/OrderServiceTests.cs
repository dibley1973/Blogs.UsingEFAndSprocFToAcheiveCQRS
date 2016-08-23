using Blogs.EfAndSprocfForCqrs.Services.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
            var customerOrderNumber = "00123";
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
        public void CreateNewOrderForCustomerWithProducts_WhenGivenCommandWithNullProductsOnOrder_ThrowsException()
        {
            // ARRANGE
            var customerId = new Guid("17e3a22e-07e5-4ab2-8e62-1b15f9916909");
            var customerOrderNumber = "00123";
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
            var customerOrderNumber = "00123";
            var command = new CreateNewOrderForCustomerWithProductsCommand
            {
                CustomerId = customerId,
                ProductsOnOrder = productsOnOrder,
                CustomerOrderNumber = customerOrderNumber
            };
            var orderService = Dependencies.Defaults.DefaultOrderService;

            // ACT
            orderService.CreateNewOrderForCustomerWithProducts(command);
        }

        [TestMethod]
        public void CreateNewOrderForCustomerWithProducts_WhenGivenValidProducts_THEN()
        {
            // ARRANGE
            var customerId = new Guid("17e3a22e-07e5-4ab2-8e62-1b15f9916909");
            var productsOnOrder = new List<int> { 8, 9 };
            var customerOrderNumber = "00123";
            var command = new CreateNewOrderForCustomerWithProductsCommand
            {
                CustomerId = customerId,
                ProductsOnOrder = productsOnOrder,
                CustomerOrderNumber = customerOrderNumber
            };
            var orderService = Dependencies.Defaults.DefaultOrderService;

            // ACT
            orderService.CreateNewOrderForCustomerWithProducts(command);

            // ASSERT
        }
    }
}