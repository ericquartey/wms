using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ferretto.VW.MAS_InverterDriver;
using Moq;
using Prism.Events;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.Common_Utils.Events;

namespace MAS_InverterDriverUnitTests
{
    [TestClass]
    public class NewInverterDriver_HorizontalMovingDrawerUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestExecuteHorizontalPosition()
        {
            var inverterDriverMock = new Mock<IInverterDriver>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<NotificationEvent>()).Returns(new NotificationEvent());

            var newInverterDriver = new NewInverterDriver(eventAggregatorMock.Object, inverterDriverMock.Object);

        }

        #endregion
    }
}
