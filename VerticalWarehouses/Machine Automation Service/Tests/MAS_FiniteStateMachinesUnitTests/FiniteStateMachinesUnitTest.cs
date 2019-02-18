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
    public class FiniteStateMachinesUnitTest
    {
        #region Methods

        [TestMethod]
        [TestCategory( "Unit" )]
        public void TestFiniteStateMachines_ExecuteVerticalHoming_Success()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var remoteIODriverMock = new Mock<INewRemoteIODriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var commandWebAPIEvent = new WebAPI_CommandEvent();
            var notifyDriverEvent = new InverterDriver_NotificationEvent();
            var notifyRemoteIODriverEvent = new RemoteIODriver_NotificationEvent();
            var notifyFSMEvent = new FiniteStateMachines_NotificationEvent();
            var machineAutomationService_Event = new MachineAutomationService_Event();

            eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<WebAPI_CommandEvent>() ).Returns( commandWebAPIEvent );
            eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<InverterDriver_NotificationEvent>() ).Returns( notifyDriverEvent );
            eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<RemoteIODriver_NotificationEvent>() ).Returns( notifyRemoteIODriverEvent );
            eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<FiniteStateMachines_NotificationEvent>() ).Returns( notifyFSMEvent );
            eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<MachineAutomationService_Event>() ).Returns( machineAutomationService_Event );

            var fsm = new FiniteStateMachines( inverterDriverMock.Object, remoteIODriverMock.Object, eventAggregatorMock.Object );

            commandWebAPIEvent.Publish( new Command_EventParameter( CommandType.ExecuteHoming ) );

            notifyDriverEvent.Publish( new Notification_EventParameter( OperationType.Homing, OperationStatus.End, "Home done", Verbosity.Info ) );

            Assert.IsNotNull( fsm.StateMachineVerticalHoming );
        }

        [TestMethod]
        [TestCategory( "Unit" )]
        public void TestFiniteStateMachinesCreate()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var remoteIODriverMock = new Mock<INewRemoteIODriver>();
            var writeLogServiceMock = new Mock<IWriteLogService>();
            var eventAggregatorMock = new Mock<IEventAggregator>();
            var machineAutomationService_Event = new MachineAutomationService_Event();

            eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<WebAPI_CommandEvent>() ).Returns( new WebAPI_CommandEvent() );
            eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<MachineAutomationService_Event>() ).Returns( machineAutomationService_Event );

            var fsm = new FiniteStateMachines( inverterDriverMock.Object, remoteIODriverMock.Object, eventAggregatorMock.Object );

            Assert.IsNotNull( fsm );
        }

        #endregion
    }
}
