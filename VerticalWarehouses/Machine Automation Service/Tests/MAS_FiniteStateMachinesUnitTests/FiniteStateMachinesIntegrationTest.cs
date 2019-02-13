using Ferretto.VW.Common_Utils.EventParameters;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_FiniteStateMachines;
using Ferretto.VW.MAS_InverterDriver;
using Ferretto.VW.MAS_IODriver;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Prism.Events;

namespace MAS_FiniteStateMachinesUnitTests
{
    [TestClass]
    public class FiniteStateMachinesIntegrationTest
    {
        #region Methods

        [TestMethod]
        public void FiniteStateMachinesExecuteVerticalHoming_Success()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var remoteIODriverMock = new Mock<INewRemoteIODriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var commandWebAPIEvent = new WebAPI_CommandEvent();
            var notifyDriverEvent = new InverterDriver_NotificationEvent();
            var notifyRemoteIODriverEvent = new RemoteIODriver_NotificationEvent();
            var notifyFSMEvent = new FiniteStateMachines_NotificationEvent();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<WebAPI_CommandEvent>()).Returns(commandWebAPIEvent);
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<InverterDriver_NotificationEvent>()).Returns(notifyDriverEvent);
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<RemoteIODriver_NotificationEvent>()).Returns(notifyRemoteIODriverEvent);
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<FiniteStateMachines_NotificationEvent>()).Returns(notifyFSMEvent);

            var fsm = new FiniteStateMachines(inverterDriverMock.Object, remoteIODriverMock.Object, writeLogServiceMock.Object, eventAggregatorMock.Object);

            commandWebAPIEvent.Publish(new Command_EventParameter(CommandType.ExecuteHoming));

            notifyDriverEvent.Publish(new Notification_EventParameter(OperationType.Homing, OperationStatus.End, "Home done", Verbosity.Info));

            Assert.IsNotNull(fsm.StateMachineVerticalHoming);
            //Assert.AreEqual(fsm.StateMachineVerticalHoming.Type, "Vertical Homing Done State");
        }

        #endregion
    }
}
