using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            //TEMP var inverterDriverMock = new Mock<INewInverterDriver>();
            //TEMP var writeLogServiceMock = new Mock<IWriteLogService>();
            //TEMP var eventAggregatorMock = new Mock<IEventAggregator>();
            //TEMP var notifyDriverEvent = new NotificationEvent();
            //TEMP eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<NotificationEvent>())
            //TEMP     .Returns(notifyDriverEvent);
            //TEMP var stateMachine = new StateMachineVerticalHoming(inverterDriverMock.Object, eventAggregatorMock.Object);

            //TEMP var state = new VerticalHomingIdleState(stateMachine, inverterDriverMock.Object,
            //TEMP     eventAggregatorMock.Object);

            //TEMP Assert.IsNotNull(state);
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestVerticalHomingIdleState_Stop()
        {
            //TEMP var inverterDriverMock = new Mock<INewInverterDriver>();
            //TEMP var eventAggregatorMock = new Mock<IEventAggregator>();
            //TEMP var notifyDriverEvent = new InverterDriver_NotificationEvent();
            //TEMP var notifyFSMEvent = new FiniteStateMachines_NotificationEvent();
            //TEMP eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<InverterDriver_NotificationEvent>()).Returns(notifyDriverEvent);
            //TEMP eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<FiniteStateMachines_NotificationEvent>()).Returns(notifyFSMEvent);

            //TEMP var stateMachine = new StateMachineVerticalHoming(inverterDriverMock.Object, eventAggregatorMock.Object);

            //TEMP var state = new VerticalHomingIdleState(stateMachine, inverterDriverMock.Object, eventAggregatorMock.Object);

            //TEMP //state.Stop();

            //TEMP Assert.AreEqual(state.Type, "Vertical Homing Idle State");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestVerticalHomingIdleState_TransitionToStateDone_Success()
        {
            //TEMP var inverterDriverMock = new Mock<INewInverterDriver>();
            //TEMP var writeLogServiceMock = new Mock<IWriteLogService>();
            //TEMP var eventAggregatorMock = new Mock<IEventAggregator>();
            //TEMP var notifyDriverEvent = new InverterDriver_NotificationEvent();
            //TEMP var notifyFSMEvent = new FiniteStateMachines_NotificationEvent();
            //TEMP eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<FiniteStateMachines_NotificationEvent>()).Returns(notifyFSMEvent);
            //TEMP eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<InverterDriver_NotificationEvent>()).Returns(notifyDriverEvent);
            //TEMP var stateMachine = new StateMachineVerticalHoming(inverterDriverMock.Object, eventAggregatorMock.Object);

            //TEMP var state = new VerticalHomingIdleState(stateMachine, inverterDriverMock.Object, eventAggregatorMock.Object);

            //TEMP notifyDriverEvent.Publish(new Notification_EventParameter(OperationType.Homing, OperationStatus.End, "Home done", Verbosity.Info));

            //TEMP Assert.AreEqual(stateMachine.Type, "Vertical Homing Done State");
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestVerticalHomingIdleState_TransitionToStateError_Success()
        {
            //TEMP var inverterDriverMock = new Mock<INewInverterDriver>();
            //TEMP var writeLogServiceMock = new Mock<IWriteLogService>();
            //TEMP var eventAggregatorMock = new Mock<IEventAggregator>();
            //TEMP var notifyDriverEvent = new NotificationEvent();
            //TEMP eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<NotificationEvent>())
            //TEMP     .Returns(notifyDriverEvent);
            //TEMP var stateMachine = new StateMachineVerticalHoming(inverterDriverMock.Object, eventAggregatorMock.Object);

            //TEMP var state = new VerticalHomingIdleState(stateMachine, inverterDriverMock.Object,
            //TEMP     eventAggregatorMock.Object);

            //TEMP notifyDriverEvent.Publish(new NotificationMessage(null, "Homing Error", MessageActor.FiniteStateMachines,
            //TEMP     MessageActor.AutomationService, MessageType.Homing, MessageStatus.OperationError,
            //TEMP     ErrorLevel.Error));

            //TEMP Assert.AreEqual(stateMachine.Type, "Vertical Homing Error State");
        }

        #endregion
    }
}
