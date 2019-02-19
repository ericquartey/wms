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
    public class VerticalHomingIdleStateUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory("Unit")]
        public void TestVerticalHomingIdleState_Create()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var notifyDriverEvent = new InverterDriver_NotificationEvent();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<InverterDriver_NotificationEvent>()).Returns(notifyDriverEvent);
            var stateMachine = new StateMachineVerticalHoming(inverterDriverMock.Object, eventAggregatorMock.Object);

            var state = new VerticalHomingIdleState(stateMachine, inverterDriverMock.Object, eventAggregatorMock.Object);

            Assert.IsNotNull(state);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestVerticalHomingIdleState_Stop()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var notifyDriverEvent = new InverterDriver_NotificationEvent();
            var notifyFSMEvent = new FiniteStateMachines_NotificationEvent();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<InverterDriver_NotificationEvent>()).Returns(notifyDriverEvent);
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<FiniteStateMachines_NotificationEvent>()).Returns(notifyFSMEvent);

            var stateMachine = new StateMachineVerticalHoming(inverterDriverMock.Object, eventAggregatorMock.Object);

            var state = new VerticalHomingIdleState(stateMachine, inverterDriverMock.Object, eventAggregatorMock.Object);

            //state.Stop();

            Assert.AreEqual(state.Type, "Vertical Homing Idle State");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestVerticalHomingIdleState_TransitionToStateDone_Success()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var notifyDriverEvent = new InverterDriver_NotificationEvent();
            var notifyFSMEvent = new FiniteStateMachines_NotificationEvent();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<FiniteStateMachines_NotificationEvent>()).Returns(notifyFSMEvent);
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<InverterDriver_NotificationEvent>()).Returns(notifyDriverEvent);
            var stateMachine = new StateMachineVerticalHoming(inverterDriverMock.Object, eventAggregatorMock.Object);

            var state = new VerticalHomingIdleState(stateMachine, inverterDriverMock.Object, eventAggregatorMock.Object);

            notifyDriverEvent.Publish(new Notification_EventParameter(OperationType.Homing, OperationStatus.End, "Home done", Verbosity.Info));

            Assert.AreEqual(stateMachine.Type, "Vertical Homing Done State");
        }

        #endregion
    }
}
