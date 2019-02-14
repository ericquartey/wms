using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines.VerticalHoming;
using Ferretto.VW.MAS_InverterDriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_FiniteStateMachinesUnitTests.VerticalHoming
{
    [TestClass]
    public class VerticalHomingDoneStateUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestVerticalHomingDoneState_Create()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var notifyFSMEvent = new FiniteStateMachines_NotificationEvent();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<FiniteStateMachines_NotificationEvent>()).Returns(notifyFSMEvent);
            var stateMachine = new StateMachineVerticalHoming(inverterDriverMock.Object, writeLogServiceMock.Object, eventAggregatorMock.Object);

            var state = new VerticalHomingDoneState(stateMachine, inverterDriverMock.Object, writeLogServiceMock.Object, eventAggregatorMock.Object);

            Assert.IsNotNull(state);
        }

        #endregion
    }
}
