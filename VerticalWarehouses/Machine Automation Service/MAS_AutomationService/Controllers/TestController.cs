using System;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Enumerations;
using Ferretto.VW.Common_Utils.Events;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerValueManagment dataLayerValueManagement;

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public TestController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerValueManagement = services.GetService(typeof(IDataLayerValueManagment)) as IDataLayerValueManagment;
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

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("DecimalConfigurationValues/{parameter}")]
        public ActionResult<decimal> GetDecimalConfigurationParameter(string parameter)
        {
            decimal returnValue;
            switch (parameter)
            {
                case "UpperBound":
                    returnValue = 1000m;
                    break;

                case "LowerBound":
                    returnValue = 10m;
                    break;

                case "Offset":
                    returnValue = 20m;
                    break;

                case "Resolution":
                    returnValue = 165.14m;
                    break;

                default:
                    var message = $"No entity with the specified parameter={parameter} exists.";
                    return this.NotFound(message);
            }
            return this.Ok(returnValue);
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
