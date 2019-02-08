using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming;
using Ferretto.VW.MAS_InverterDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_FiniteStateMachinesUnitTests
{
    [TestClass]
    public class StateMachineVerticalHomingTest
    {
        #region Methods

        [TestMethod]
        public void StateMachineVerticalHomingCreate()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();

            var smVerticalHoming = new StateMachineVerticalHoming(inverterDriverMock.Object, writeLogServiceMock.Object, eventAggregatorMock.Object);

            Assert.IsNotNull(smVerticalHoming);
        }

        [TestMethod]
        public void StateMachineVerticalHomingStartSuccess()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var notifyDriverEvent = new InverterDriver_NotificationEvent();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<InverterDriver_NotificationEvent>()).Returns(notifyDriverEvent);

            var smVerticalHoming = new StateMachineVerticalHoming(inverterDriverMock.Object, writeLogServiceMock.Object, eventAggregatorMock.Object);
            smVerticalHoming.Start();

            Assert.AreEqual(smVerticalHoming.Type, "Vertical Homing Idle State");
        }

        #endregion
    }
}
