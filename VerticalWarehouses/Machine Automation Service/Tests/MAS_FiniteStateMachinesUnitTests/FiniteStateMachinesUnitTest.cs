using System.Threading;
using Ferretto.VW.Common_Utils.Events;
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
        [TestCategory("Unit")]
        public void TestFiniteStateMachines_ExecuteVerticalHoming_Success()
        {
            //TEMP var inverterDriverMock = new Mock<INewInverterDriver>();
            //TEMP var remoteIODriverMock = new Mock<INewRemoteIODriver>();
            //TEMP var writeLogServiceMock = new Mock<IWriteLogService>();
            //TEMP var eventAggregatorMock = new Mock<IEventAggregator>();
            //TEMP var commandWebAPIEvent = new WebAPI_CommandEvent();
            //TEMP var notifyDriverEvent = new InverterDriver_NotificationEvent();
            //TEMP var notifyRemoteIODriverEvent = new RemoteIODriver_NotificationEvent();
            //TEMP var notifyFSMEvent = new FiniteStateMachines_NotificationEvent();
            //TEMP var machineAutomationService_Event = new MachineAutomationService_Event();

            //TEMP eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<WebAPI_CommandEvent>() ).Returns( commandWebAPIEvent );
            //TEMP eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<InverterDriver_NotificationEvent>() ).Returns( notifyDriverEvent );
            //TEMP eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<RemoteIODriver_NotificationEvent>() ).Returns( notifyRemoteIODriverEvent );
            //TEMP eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<FiniteStateMachines_NotificationEvent>() ).Returns( notifyFSMEvent );
            //TEMP eventAggregatorMock.Setup( aggregator => aggregator.GetEvent<MachineAutomationService_Event>() ).Returns( machineAutomationService_Event );

            //TEMP var fsm = new FiniteStateMachines( inverterDriverMock.Object, remoteIODriverMock.Object, eventAggregatorMock.Object );

            //TEMP commandWebAPIEvent.Publish( new Command_EventParameter( CommandType.ExecuteHoming ) );

            //TEMP notifyDriverEvent.Publish( new Notification_EventParameter( OperationType.Homing, OperationStatus.End, "Home done", Verbosity.Info ) );

            //TEMP Assert.IsNotNull( fsm.StateMachineVerticalHoming );
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void TestFiniteStateMachinesCreate()
        {
            var inverterDriverMock = new Mock<INewInverterDriver>();
            var remoteIODriverMock = new Mock<INewRemoteIODriver>();
            var eventAggregatorMock = new Mock<IEventAggregator>();

            var commandServiceEvent = new CommandEvent();
            eventAggregatorMock.Setup(aggregator => aggregator.GetEvent<CommandEvent>()).Returns(commandServiceEvent);

            var fsm = new FiniteStateMachines(inverterDriverMock.Object, remoteIODriverMock.Object, eventAggregatorMock.Object);

            fsm.StartAsync(new CancellationToken()).Wait();

            Assert.IsNotNull(fsm);
        }

        #endregion
    }
}
