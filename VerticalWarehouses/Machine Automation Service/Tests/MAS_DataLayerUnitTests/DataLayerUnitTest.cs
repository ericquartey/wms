using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ferretto.VW.MAS_DataLayer;
using Prism.Events;
using Moq;
using System;
using Microsoft.Extensions.Configuration;

namespace MAS_DataLayerUnitTests
{
    [TestClass]
    public class DataLayerUnitTest : DBTestUnitTest
    {
        [TestMethod]
        public void TestMethodDataLayer()
        {
            using (var context = this.CreateContext())
            {
                // Arrange
                Mock<IConfiguration> mockConfiguration = new Mock<IConfiguration>();
                // Insert the Mock here
                mockConfiguration.Setup(s => s.GetConnectionString(It.IsAny<string>())).Returns("AutomationService");

                new DataLayer( mockConfiguration.Object, context );

                // Act

                // Assert
                Assert.IsFalse( context.Database.ProviderName.Equals("InMemory") );
            }
        }
    }
}
