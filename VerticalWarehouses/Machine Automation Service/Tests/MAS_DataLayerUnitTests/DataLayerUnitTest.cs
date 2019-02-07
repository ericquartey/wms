using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ferretto.VW.MAS_DataLayer;
using Prism.Events;
using Moq;
using System;
using Castle.Core.Configuration;

namespace MAS_DataLayerUnitTests
{
    [TestClass]
    public class DataLayerUnitTest : DBTest
    {
        [TestMethod]
        public void DataLayerTestMethod()
        {
            using (var context = this.CreateContext())
            {
                // Arrange
                Mock<IConfiguration> mockConfiguration = new Mock<IConfiguration>();

                // Act

                // Assert

            }
        }
    }
}
