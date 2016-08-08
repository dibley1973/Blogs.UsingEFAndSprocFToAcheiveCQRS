using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using System.Linq;

namespace Blogs.EfAndSprocfForCqrs.IntegrationTests
{
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
    }
}