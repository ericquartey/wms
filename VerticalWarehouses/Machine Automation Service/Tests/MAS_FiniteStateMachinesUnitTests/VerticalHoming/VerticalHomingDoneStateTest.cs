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
    public class VerticalHomingDoneStateTest
    {
        #region Methods

        [TestMethod]
        public void VerticalHomingDoneStateTestCreate()
        {
            var inverterDriverMock = new Mock<IInverterDriver>();
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
