using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.CommonUtils.Messages.Interfaces;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public partial class TestController : BaseAutomationController
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagment;

        private readonly IEventAggregator eventAggregator;

        #endregion

        #region Constructors

        public TestController(
            IEventAggregator eventAggregator,
            IConfigurationValueManagmentDataLayer configurationValueManagmentDataLayer)
            : base(eventAggregator)
        {
            if (eventAggregator is null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (configurationValueManagmentDataLayer is null)
            {
                throw new ArgumentNullException(nameof(configurationValueManagmentDataLayer));
            }

            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagment = configurationValueManagmentDataLayer;
        }

        #endregion

        #region Methods

        [HttpGet("BayNowServiceable")]
        public void BayNowServiceable()
        {
            this.PublishNotification(
                null,
                "Test bay now serviceable",
                MessageActor.MissionsManager,
                MessageType.MissionOperationCompleted,
                MessageStatus.OperationEnd);
        }

        [HttpGet("HomingTest")]
        public async void ExecuteHoming()
        {
            this.PublishNotification(
             null,
             "Homing Started",
             MessageActor.AutomationService,
             MessageActor.FiniteStateMachines,
             MessageType.Homing,
             MessageStatus.OperationStart);

            await Task.Delay(2000);

            this.PublishNotification(
                null,
                "Switching Engine Started",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.SwitchAxis,
                MessageStatus.OperationEnd);

            await Task.Delay(2000);
        }

        [HttpPost]
        [Route("ExecuteResolutionCalibration/{readInitialPosition}/{readFinalPosition}")]
        public async Task ExecuteResolutionCalibrationAsync(decimal readInitialPosition, decimal readFinalPosition)
        {
            var resolutionCalibrationMessageData = new ResolutionCalibrationMessageData(readInitialPosition, readFinalPosition);

            this.PublishNotification(
                resolutionCalibrationMessageData,
                "Resolution Calibration Started",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.ResolutionCalibration,
                MessageStatus.OperationStart);

            await Task.Delay(2000);

            resolutionCalibrationMessageData.Resolution = 1.0001m;

            this.PublishNotification(
                resolutionCalibrationMessageData,
                "Resolution Calibration Ended",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.ResolutionCalibration,
                MessageStatus.OperationEnd);
        }

        [HttpPost]
        public async Task ExecuteShutterPositioningMovementTestAsync(
            [FromBody]ShutterPositioningMovementMessageDataDto data)
        {
            await this.ExecuteShutterPositioningMovementMethod();
        }

        [HttpGet("HomingStop")]
        public void ExecuteStopHoming()
        {
            this.PublishCommand(
                null,
                "Stop Homing",
                MessageActor.FiniteStateMachines,
                MessageActor.AutomationService,
                MessageType.Stop);
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("DecimalConfigurationValues/{parameter}")]
        public ActionResult<decimal> GetDecimalConfigurationParameter(string parameter)
        {
            return this.GetDecimalConfigurationParameterMethod(parameter);
        }

        [ProducesResponseType(200, Type = typeof(bool))]
        [ProducesResponseType(500)]
        [HttpGet("GetInstallationStatus")]
        public ActionResult<bool[]> GetInstallationStatus()
        {
            return this.GetInstallationStatusMethod();
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        public ActionResult<int> GetIntegerConfigurationParameter(string category, string parameter)
        {
            return this.GetIntegerConfigurationParameterMethod(category, parameter);
        }

        [HttpGet("Homing")]
        public void Homing()
        {
            this.HomingMethod();
        }

        [HttpGet("HorizontalPositioning")]
        public void HorizontalPositioning()
        {
            this.HorizontalPositioningMethod();
        }

        [HttpGet("StartShutterControl/{bayNumber}/{delay}/{numberCycles}")]
        public async Task StartShutterControlAsync(int bayNumber, int delay, int numberCycles)
        {
            await this.StartShutterControlMethod();
        }

        [HttpGet("StartShutterControlError/{delay}/{numberCycles}")]
        public void StartShutterControlError(int delay, int numberCycles)
        {
            this.StartShutterControlErrorMethod(delay, numberCycles);
        }

        [HttpGet("StopFSM")]
        public void StopFiniteStateMachine()
        {
            this.StopFiniteStateMachineMethod();
        }

        [HttpGet("UpdateCurrentPositionTest")]
        public async Task UpdateCurrentPositionTest()
        {
            await this.UpdateCurrentPositionTestMethod();
        }

        [HttpGet("VerticalPositioning")]
        public void VerticalPositioning()
        {
            this.VerticalPositioningMethod();
        }

        protected void PublishNotification(
                                                                                                                         IMessageData data,
         string description,
         MessageActor receiver,
         MessageActor sender,
         MessageType type,
         MessageStatus status,
         ErrorLevel level = ErrorLevel.NoError)
        {
            this.eventAggregator
                .GetEvent<NotificationEvent>()
                .Publish(
                    new NotificationMessage(
                        data,
                        description,
                        receiver,
                        sender,
                        type,
                        status,
                        level));
        }

        private async Task ExecuteShutterPositioningMovementMethod()
        {
            var speedRate = 1.2m;
            var dto = new ShutterPositioningMovementMessageDataDto(ShutterMovementDirection.Up, 1);
            dto.ShutterType = ShutterType.NoType;
            var dataInterface = new ShutterPositioningMessageData(ShutterPosition.Opened, dto.ShutterPositionMovement, dto.ShutterType, dto.BayNumber, speedRate);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(
                dataInterface,
                "Shutter Positioning Started",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                MessageStatus.OperationStart));

            await Task.Delay(2000);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(
                dataInterface,
                "Shutter Positioning Completed",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterPositioning,
                MessageStatus.OperationEnd));
        }

        private ActionResult<decimal> GetDecimalConfigurationParameterMethod(string parameter)
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

        private ActionResult<bool[]> GetInstallationStatusMethod()
        {
            bool[] installationStatus = { true, false, true, false, false, true, false, true, false, true, false, false, false, false, false };

            return this.Ok(installationStatus);
        }

        private ActionResult<int> GetIntegerConfigurationParameterMethod(string categoryString, string parameter)
        {
            var category = (ConfigurationCategory)Enum.Parse(typeof(ConfigurationCategory), categoryString);

            long longParameter;

            switch (category)
            {
                case ConfigurationCategory.GeneralInfo:
                    {
                        longParameter = (long)Enum.Parse(typeof(GeneralInfo), parameter);
                        break;
                    }
                default:
                    {
                        longParameter = 0;
                        break;
                    }
            }

            var returnValue = this.dataLayerConfigurationValueManagment
                .GetIntegerConfigurationValue(longParameter, category);

            return this.Ok(returnValue);
        }

        private void HomingMethod()
        {
            var messageData = new HomingMessageData(Axis.Both);

            this.PublishCommand(
                messageData,
                "Homing",
                MessageActor.FiniteStateMachines,
                MessageType.Homing);
        }

        private void HorizontalPositioningMethod()
        {
            var messageData = new PositioningMessageData(Axis.Horizontal, MovementType.Relative, MovementMode.Position, 4096m, 200m, 200m, 200m, 0, 0, 0, 0);

            this.PublishCommand(
                messageData,
                "Horizontal relative positioning",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);
        }

        private void PublishCommand(
            IMessageData messageData,
            string description,
            MessageActor receiver,
            MessageActor sender,
            MessageType messageType)
        {
            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(
                    new CommandMessage(
                        messageData,
                        description,
                        receiver,
                        sender,
                        messageType));
        }

        private void StartShutterControlErrorMethod(int delay, int numberCycles)
        {
            var bayNumber = 2;
            var speed = 100;

            var dataInterface = new ShutterTestStatusChangedMessageData(bayNumber, delay, numberCycles, speed);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(
                dataInterface,
                "Simulated Shutter Error",
                 MessageActor.AutomationService,
                 MessageActor.FiniteStateMachines,
                 MessageType.ShutterTestStatusChanged,
                 MessageStatus.OperationError));
        }

        private async Task StartShutterControlMethod()
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(
                null,
                "Shutter Started",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterTestStatusChanged,
                MessageStatus.OperationStart));

            await Task.Delay(2000);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(
                null,
                "Shutter Completed",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterTestStatusChanged,
                MessageStatus.OperationEnd));
        }

        private void StopFiniteStateMachineMethod()
        {
            this.PublishCommand(
                null,
                "Stop Homing",
                MessageActor.FiniteStateMachines,
                MessageActor.AutomationService,
                MessageType.Stop);
        }

        private async Task UpdateCurrentPositionTestMethod()
        {
            for (var i = 0; i <= 350; i += 50)
            {
                var positionData = new CurrentPositionMessageData(i);

                this.PublishNotification(
                    positionData,
                    "Update current position",
                    MessageActor.AutomationService,
                    MessageActor.FiniteStateMachines,
                    MessageType.Positioning,
                    MessageStatus.OperationExecuting);

                await Task.Delay(1000);
            }
        }

        private void VerticalPositioningMethod()
        {
            var messageData = new PositioningMessageData(Axis.Vertical, MovementType.Relative, MovementMode.Position, 4096m, 200m, 200m, 200m, 0, 0, 0, 0);
            var message = new CommandMessage(messageData, "Vertical relative positioning", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        #endregion
    }
}
