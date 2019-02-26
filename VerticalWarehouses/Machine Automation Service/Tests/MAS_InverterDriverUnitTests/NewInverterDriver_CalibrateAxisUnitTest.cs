using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.MAS_InverterDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_InverterDriverUnitTests
{
    [TestClass]
    public class NewInverterDriver_CalibrateAxisUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestExecuteHorizontalHoming()
        {
            var inverterDriverMock = new Mock<IInverterDriver>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<NotificationEvent>())
                .Returns(new NotificationEvent());

            var newInverterDriver = new NewInverterDriver(eventAggregatorMock.Object, inverterDriverMock.Object);
            newInverterDriver.ExecuteHorizontalHoming();
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestExecuteVerticalHoming()
        {
            var inverterDriverMock = new Mock<IInverterDriver>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<NotificationEvent>())
                .Returns(new NotificationEvent());

            var newInverterDriver = new NewInverterDriver(eventAggregatorMock.Object, inverterDriverMock.Object);
            newInverterDriver.ExecuteVerticalHoming();
        }

        #endregion
    }
}
