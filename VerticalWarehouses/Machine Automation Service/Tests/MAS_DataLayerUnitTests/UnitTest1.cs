using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ferretto.VW.MAS_DataLayer;
using Prism.Events;
using Moq;

namespace MAS_DataLayerUnitTests
{
    [TestClass]
    public class UnitTest1 : DBTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            using (var context = this.CreateContext())
            {
                // Arrange
                bool updateFeedback = false;
                Mock<IEventAggregator> mockEventAggregator = new Mock<IEventAggregator>();
                var writeLogService = new WriteLogService(context, mockEventAggregator.Object);

                // Act
                updateFeedback = writeLogService.LogWriting("Unit Test");

                // Assert
                Assert.IsTrue(updateFeedback);
            }
        }
    }
}
