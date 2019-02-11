using Ferretto.VW.Common_Utils.EventParameters;
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
    public class VerticalHomingIdleStateTest
    {
        #region Methods

        [TestMethod]
        public void VerticalHomingIdleStateTest_TransitionToStateDone_Success()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var notifyDriverEvent = new InverterDriver_NotificationEvent();
            var notifyFSMEvent = new FiniteStateMachines_NotificationEvent();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<FiniteStateMachines_NotificationEvent>()).Returns(notifyFSMEvent);
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<InverterDriver_NotificationEvent>()).Returns(notifyDriverEvent);
            var stateMachine = new StateMachineVerticalHoming(inverterDriverMock.Object, writeLogServiceMock.Object, eventAggregatorMock.Object);

            var state = new VerticalHomingIdleState(stateMachine, inverterDriverMock.Object, writeLogServiceMock.Object, eventAggregatorMock.Object);

            notifyDriverEvent.Publish(new Notification_EventParameter(OperationType.Homing, OperationStatus.End, "Home done", Verbosity.Info));

            Assert.AreEqual(stateMachine.Type, "Vertical Homing Done State");
        }

        [TestMethod]
        public void VerticalHomingIdleStateTestCreate()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var notifyDriverEvent = new InverterDriver_NotificationEvent();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<InverterDriver_NotificationEvent>()).Returns(notifyDriverEvent);
            var stateMachine = new StateMachineVerticalHoming(inverterDriverMock.Object, writeLogServiceMock.Object, eventAggregatorMock.Object);

            var state = new VerticalHomingIdleState(stateMachine, inverterDriverMock.Object, writeLogServiceMock.Object, eventAggregatorMock.Object);

            Assert.IsNotNull(state);
        }

        #endregion
    }
}
