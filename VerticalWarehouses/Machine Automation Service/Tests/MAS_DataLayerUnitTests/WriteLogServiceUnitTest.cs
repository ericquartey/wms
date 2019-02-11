using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ferretto.VW.MAS_DataLayer;
using Prism.Events;
using Moq;
using Ferretto.VW.Common_Utils.Events;

namespace MAS_DataLayerUnitTests
{
    [TestClass]
    public class WriteLogServiceUnitTest : DBTestUnitTest
    {
        #region Methods

        [TestMethod]
        public void TestMethodWriteLogService()
        {
            using (var context = this.CreateContext())
            {
                // Arrange
                bool updateFeedback = false;
                Mock<IEventAggregator> mockEventAggregator = new Mock<IEventAggregator>();
                mockEventAggregator.Setup(s => s.GetEvent<WebAPI_CommandEvent>()).Returns(new WebAPI_CommandEvent());

                var writeLogService = new WriteLogService(context, mockEventAggregator.Object);

                // Act
                updateFeedback = writeLogService.LogWriting("Unit Test");

                // Assert
                Assert.IsTrue(updateFeedback);
            }
        }

        #endregion
    }
}
