using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController
    {
        #region Fields

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public TestController(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
        }

        #endregion

        #region Methods

        [HttpGet("AddMissionTest")]
        public void AddMission()
        {
            var missionData = new MissionMessageData(1, 1, 1, MissionType.CellToBay, 1);
            var missionMessage = new CommandMessage(missionData,
                "Test Mission",
                MessageActor.AutomationService,
                MessageActor.WebAPI,
                MessageType.AddMission,
                MessageVerbosity.Debug);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(missionMessage);
        }

        [HttpPost("CreateMissionTest")]
        public void CreateMission([FromBody] int bayID, int drawerID)
        {
            var missionData = new MissionMessageData(1, 1, 1, MissionType.CellToBay, 1);

            var message = new CommandMessage(missionData,
                "Create Mission",
                MessageActor.MissionsManager,
                MessageActor.WebAPI,
                MessageType.CreateMission,
                MessageVerbosity.Debug);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        [HttpGet("HomingTest")]
        public async void ExecuteHoming()
        {
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Homing Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OperationStart));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Horizontal Homing Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.HorizontalHoming, MessageStatus.OperationStart));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Horizontal Homing Executing", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.HorizontalHoming, MessageStatus.OperationExecuting));
            await Task.Delay(4000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Horizontal Homing Ended", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.HorizontalHoming, MessageStatus.OperationEnd));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Switching Engine Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.SwitchAxis, MessageStatus.OperationStart));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Switching Engine Ended", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.SwitchAxis, MessageStatus.OperationEnd));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Vertical Homing Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.VerticalHoming, MessageStatus.OperationStart));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Vertical Homing Executing", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.VerticalHoming, MessageStatus.OperationExecuting));
            await Task.Delay(4000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Vertical Homing Ended", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.VerticalHoming, MessageStatus.OperationEnd));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Switching Engine Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.SwitchAxis, MessageStatus.OperationStart));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Switching Engine Ended", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.SwitchAxis, MessageStatus.OperationEnd));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Horizontal Homing Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.HorizontalHoming, MessageStatus.OperationStart));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Horizontal Homing Executing", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.HorizontalHoming, MessageStatus.OperationExecuting));
            await Task.Delay(4000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Horizontal Homing Ended", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.HorizontalHoming, MessageStatus.OperationEnd));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Homing Completed", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OperationEnd));
        }

        [HttpGet("HomingStop")]
        public void ExecuteStopHoming()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Stop Homing",
                MessageActor.FiniteStateMachines, MessageActor.AutomationService, MessageType.Stop,
                MessageVerbosity.Info));
        }

        [HttpGet("MissionExecutedTest")]
        public void MissionExecuted()
        {
            var message = new NotificationMessage(
                null,
                "Mission Executed",
                MessageActor.MissionsManager,
                MessageActor.FiniteStateMachines,
                MessageType.EndAction,
                MessageStatus.OperationEnd);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(message);
        }

        [HttpGet("StopFSM")]
        public void StopFiniteStateMachine()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Stop Homing",
                MessageActor.FiniteStateMachines, MessageActor.AutomationService, MessageType.Stop,
                MessageVerbosity.Info));
        }

        #endregion
    }
}
