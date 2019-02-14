using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ferretto.VW.MAS_InverterDriver;
using Moq;
using Prism.Events;
using Ferretto.VW.InverterDriver;
using Ferretto.VW.Common_Utils.Events;

namespace MAS_InverterDriverUnitTests
{
    [TestClass]
    public class NewInverterDriverUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestNewInverterDriverCreate()
        {
            var inverterDriverMock = new Mock<IInverterDriver>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<InverterDriver_NotificationEvent>()).Returns(new InverterDriver_NotificationEvent());
            inverterDriverMock.Setup(x => x.Brake_Resistance_Overtemperature).Returns(true);

            var newInverterDriver = new NewInverterDriver(eventAggregatorMock.Object, inverterDriverMock.Object);
            var result = newInverterDriver.GetSensorsStates();

            Assert.IsNotNull(newInverterDriver);
            Assert.IsTrue(result[0]);
        }

        #endregion
    }
}
