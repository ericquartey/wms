using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ferretto.VW.MAS_DataLayer;
using Prism.Events;
using Moq;
using System;
using Microsoft.Extensions.Configuration;

namespace MAS_DataLayerUnitTests
{
    [TestClass]
    public class DataLayerGetSetUnitTest : DBTest
    {
        #region Methods

        [TestMethod]
        public void DataLayerTestMethod()
        {
            using (var context = this.CreateContext())
            {
                // Arrange
                Mock<IConfiguration> mockConfiguration = new Mock<IConfiguration>();
                // Insert the Mock here
                mockConfiguration.Setup(s => s.GetConnectionString(It.IsAny<string>())).Returns("AutomationService");

                var dataLayer = new DataLayer(mockConfiguration.Object, context);

                // Act
                dataLayer.SetIntConfigurationValue(ConfigurationValueEnum.homingCreepSpeed, 5);

                // Assert
                Assert.IsFalse(dataLayer.GetIntConfigurationValue(ConfigurationValueEnum.homingCreepSpeed) == 5);
            }
        }

        #endregion
    }
}
