using System;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagment;

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public TestController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagment = services.GetService(typeof(IDataLayerConfigurationValueManagment)) as IDataLayerConfigurationValueManagment;
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
                MessageActor.WebApi,
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
                MessageActor.WebApi,
                MessageType.CreateMission,
                MessageVerbosity.Debug);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        [HttpGet("HomingTest")]
        public async void ExecuteHoming()
        {
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Homing Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.CalibrateAxis, MessageStatus.OperationStart));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Switching Engine Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.SwitchAxis, MessageStatus.OperationEnd));
            await Task.Delay(2000);

            return;

            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Homing Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OperationStart));
            await Task.Delay(2000);
            //this.eventAggregator.GetEvent<NotificationEvent>()
            //    .Publish(new NotificationMessage(null, "Horizontal Homing Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.HorizontalHoming, MessageStatus.OperationStart));
            //await Task.Delay(2000);
            //this.eventAggregator.GetEvent<NotificationEvent>()
            //    .Publish(new NotificationMessage(null, "Horizontal Homing Executing", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.HorizontalHoming, MessageStatus.OperationExecuting));
            //await Task.Delay(2000);

            //TEMP this.eventAggregator.GetEvent<NotificationEvent>()
            //TEMP     .Publish(new NotificationMessage(null, "Horizontal Homing Error", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OperationError));
            //TEMP await Task.Delay(2000);

            //this.eventAggregator.GetEvent<NotificationEvent>()
            //    .Publish(new NotificationMessage(null, "Horizontal Homing Ended", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.HorizontalHoming, MessageStatus.OperationEnd));
            //await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Switching Engine Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.SwitchAxis, MessageStatus.OperationStart));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Switching Engine Ended", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.SwitchAxis, MessageStatus.OperationEnd));
            await Task.Delay(2000);
            //this.eventAggregator.GetEvent<NotificationEvent>()
            //    .Publish(new NotificationMessage(null, "Vertical Homing Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.VerticalHoming, MessageStatus.OperationStart));
            //await Task.Delay(2000);
            //this.eventAggregator.GetEvent<NotificationEvent>()
            //    .Publish(new NotificationMessage(null, "Vertical Homing Executing", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.VerticalHoming, MessageStatus.OperationExecuting));
            //await Task.Delay(4000);
            //this.eventAggregator.GetEvent<NotificationEvent>()
            //    .Publish(new NotificationMessage(null, "Vertical Homing Ended", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.VerticalHoming, MessageStatus.OperationEnd));
            //await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Switching Engine Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.SwitchAxis, MessageStatus.OperationStart));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Switching Engine Ended", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.SwitchAxis, MessageStatus.OperationEnd));
            await Task.Delay(2000);
            //this.eventAggregator.GetEvent<NotificationEvent>()
            //    .Publish(new NotificationMessage(null, "Horizontal Homing Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.HorizontalHoming, MessageStatus.OperationStart));
            //await Task.Delay(2000);
            //this.eventAggregator.GetEvent<NotificationEvent>()
            //    .Publish(new NotificationMessage(null, "Horizontal Homing Executing", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.HorizontalHoming, MessageStatus.OperationExecuting));
            //await Task.Delay(4000);
            //this.eventAggregator.GetEvent<NotificationEvent>()
            //    .Publish(new NotificationMessage(null, "Horizontal Homing Ended", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.HorizontalHoming, MessageStatus.OperationEnd));
            //await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>()
                .Publish(new NotificationMessage(null, "Homing Completed", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OperationEnd));
        }

        [HttpPost]
        public async Task ExecuteShutterPositioningMovementTestAsync([FromBody]ShutterPositioningMovementMessageDataDTO data)
        {
            var dto = new ShutterPositioningMovementMessageDataDTO(1, ShutterMovementDirection.Up);
            dto.ShutterType = 1;
            var dataInterface = new ShutterPositioningMessageData(dto.ShutterPositionMovement);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(dataInterface, "Shutter Positioning Started",
                 MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.ShutterPositioning,
                MessageStatus.OperationStart));

            await Task.Delay(2000);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(dataInterface, "Shutter Positioning Completed",
                MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.ShutterPositioning,
                MessageStatus.OperationEnd));
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

        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(500)]
        [HttpGet("GetInstallationStatus")]
        public ActionResult<bool[]> GetInstallationStatus()
        {
            bool[] installationStatus = { true, false, true, false, false, true, false, true, false, true, false, false, false, false, false };
            if (installationStatus != null)
            {
                return this.Ok(installationStatus);
            }
            else
            {
                return this.StatusCode(500);
            }
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        public ActionResult<int> GetIntegerConfigurationParameter(string category, string parameter)
        {
            var categoryEnum = (ConfigurationCategory)Enum.Parse(typeof(ConfigurationCategory), category);

            long longParameter;
            long longCategory;

            switch (categoryEnum)
            {
                case ConfigurationCategory.GeneralInfo:
                    {
                        longCategory = (long)categoryEnum;
                        longParameter = (long)Enum.Parse(typeof(GeneralInfo), parameter);
                        break;
                    }
                default:
                    {
                        longParameter = 0;
                        longCategory = 0;
                        break;
                    }
            }

            var returnValue = this.dataLayerConfigurationValueManagment.GetIntegerConfigurationValueAsync(longParameter, longCategory);

            return this.Ok(returnValue);
        }

        [HttpGet("Homing")]
        public async void Homing()
        {
            var messageData = new HomingMessageData(Axis.Both);
            var message = new CommandMessage(messageData, "Homing", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Homing);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        [HttpGet("HorizontalPositioning")]
        public void HorizontalPositioning()
        {
            var messageData = new PositioningMessageData(Axis.Horizontal, MovementType.Relative, 4096m, 200m, 200m, 200m, 0, 0, 0);
            var message = new CommandMessage(messageData, "Horizontal relative positioning", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        [HttpGet("MissionExecutedTest")]
        public void MissionExecuted()
        {
            //var message = new NotificationMessage(
            //    null,
            //    "Mission Executed",
            //    MessageActor.MissionsManager,
            //    MessageActor.FiniteStateMachines,
            //    MessageType.EndAction,
            //    MessageStatus.OperationEnd);
            //this.eventAggregator.GetEvent<NotificationEvent>().Publish(message);
        }

        [HttpGet("ResetIO")]
        public void ResetIO()
        {
            //this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "ResetIO",
            //    MessageActor.IODriver, MessageActor.AutomationService, MessageType.IOReset,
            //    MessageVerbosity.Info));
        }

        [HttpGet("StartShutterControl/{delay}/{numberCycles}")]
        public async Task StartShutterControlAsync(int delay, int numberCycles)
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null, "Shutter Started",
                 MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.ShutterControl,
                MessageStatus.OperationStart));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null, "Shutter Completed",
                MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Stop,
                MessageStatus.OperationEnd));
        }

        [HttpGet("StartShutterControlError/{delay}/{numberCycles}")]
        public void StartShutterControlError(int delay, int numberCycles)
        {
            var dataInterface = new ShutterControlMessageData(delay, numberCycles);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(dataInterface,
                "Simulated Shutter Error",
                 MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.ShutterControl,
                 MessageStatus.OperationError));
        }

        [HttpGet("StopFSM")]
        public void StopFiniteStateMachine()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Stop Homing",
                MessageActor.FiniteStateMachines, MessageActor.AutomationService, MessageType.Stop,
                MessageVerbosity.Info));
        }

        [HttpGet("UpdateCurrentPositionTest")]
        public async Task UpdateCurrentPositionTest()
        {
            var notificationEvent = this.eventAggregator.GetEvent<NotificationEvent>();
            var positionData = new CurrentPositionMessageData(0m);
            notificationEvent.Publish(new NotificationMessage(
                positionData, "Update current position", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Positioning, MessageStatus.OperationExecuting));
            await Task.Delay(1000);
            positionData = new CurrentPositionMessageData(50m);
            notificationEvent.Publish(new NotificationMessage(
                positionData, "Update current position", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Positioning, MessageStatus.OperationExecuting));
            await Task.Delay(1000);
            positionData = new CurrentPositionMessageData(100m);
            notificationEvent.Publish(new NotificationMessage(
                positionData, "Update current position", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Positioning, MessageStatus.OperationExecuting));
            await Task.Delay(1000);
            positionData = new CurrentPositionMessageData(150m);
            notificationEvent.Publish(new NotificationMessage(
                positionData, "Update current position", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Positioning, MessageStatus.OperationExecuting));
            await Task.Delay(1000);
            positionData = new CurrentPositionMessageData(200m);
            notificationEvent.Publish(new NotificationMessage(
                positionData, "Update current position", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Positioning, MessageStatus.OperationExecuting));
            await Task.Delay(1000);
            positionData = new CurrentPositionMessageData(250m);
            notificationEvent.Publish(new NotificationMessage(
                positionData, "Update current position", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Positioning, MessageStatus.OperationExecuting));
            await Task.Delay(1000);
            positionData = new CurrentPositionMessageData(300m);
            notificationEvent.Publish(new NotificationMessage(
                positionData, "Update current position", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Positioning, MessageStatus.OperationExecuting));
            await Task.Delay(1000);
            positionData = new CurrentPositionMessageData(350m);
            notificationEvent.Publish(new NotificationMessage(
                positionData, "Update current position", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Positioning, MessageStatus.OperationExecuting));
        }

        [HttpGet("VerticalPositioning")]
        public void VerticalPositioning()
        {
            var messageData = new PositioningMessageData(Axis.Vertical, MovementType.Relative, 4096m, 200m, 200m, 200m, 0, 0, 0);
            var message = new CommandMessage(messageData, "Vertical relative positioning", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        #endregion
    }
}
