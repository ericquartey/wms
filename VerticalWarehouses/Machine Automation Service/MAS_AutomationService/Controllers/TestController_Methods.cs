using System;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.DTOs;
using Ferretto.VW.CommonUtils.Messages;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    public partial class TestController
    {
        #region Methods

        private void BayNowServiceableMethod()
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null, "Test bay now serviceable", MessageActor.MissionsManager, MessageActor.WebApi, MessageType.MissionOperationCompleted, MessageStatus.OperationEnd));
        }

        private async Task ExecuteHomingMethod()
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(
                null,
                "Homing Started",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.Homing,
                MessageStatus.OperationStart));

            await Task.Delay(2000);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(
                null,
                "Switching Engine Started",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.SwitchAxis,
                MessageStatus.OperationEnd));

            await Task.Delay(2000);
        }

        private async Task ExecuteResolutionCalibrationMethod(decimal readInitialPosition, decimal readFinalPosition)
        {
            var resolutionCalibrationMessageData = new ResolutionCalibrationMessageData(readInitialPosition, readFinalPosition);
            var notificationMessage = new NotificationMessage(resolutionCalibrationMessageData, "Resolution Calibration Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.ResolutionCalibration, MessageStatus.OperationStart);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
            await Task.Delay(2000);
            resolutionCalibrationMessageData.Resolution = 1.0001m;
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(
                resolutionCalibrationMessageData,
                "Resolution Calibration Ended",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.ResolutionCalibration,
                MessageStatus.OperationEnd));
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

        private void ExecuteStopHomingMethod()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(
                null,
                "Stop Homing",
                MessageActor.FiniteStateMachines,
                MessageActor.AutomationService,
                MessageType.Stop));
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
            if (installationStatus != null)
            {
                return this.Ok(installationStatus);
            }
            else
            {
                return this.StatusCode(500);
            }
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
            var message = new CommandMessage(messageData, "Homing", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Homing);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        private void HorizontalPositioningMethod()
        {
            var messageData = new PositioningMessageData(Axis.Horizontal, MovementType.Relative, MovementMode.Position, 4096m, 200m, 200m, 200m, 0, 0, 0, 0);
            var message = new CommandMessage(messageData, "Horizontal relative positioning", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        private void StartShutterControlErrorMethod(int delay, int numberCycles)
        {
            var bayNumber = 2; var speed = 100;
            var dataInterface = new ShutterControlMessageData(bayNumber, delay, numberCycles, speed);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(
                dataInterface,
                "Simulated Shutter Error",
                 MessageActor.AutomationService,
                 MessageActor.FiniteStateMachines,
                 MessageType.ShutterControl,
                 MessageStatus.OperationError));
        }

        private async Task StartShutterControlMethod()
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(
                null,
                "Shutter Started",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterControl,
                MessageStatus.OperationStart));

            await Task.Delay(2000);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(
                null,
                "Shutter Completed",
                MessageActor.AutomationService,
                MessageActor.FiniteStateMachines,
                MessageType.ShutterControl,
                MessageStatus.OperationEnd));
        }

        private void StopFiniteStateMachineMethod()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(
                null,
                "Stop Homing",
                MessageActor.FiniteStateMachines,
                MessageActor.AutomationService,
                MessageType.Stop));
        }

        private async Task UpdateCurrentPositionTestMethod()
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

        private void VerticalPositioningMethod()
        {
            var messageData = new PositioningMessageData(Axis.Vertical, MovementType.Relative, MovementMode.Position, 4096m, 200m, 200m, 200m, 0, 0, 0, 0);
            var message = new CommandMessage(messageData, "Vertical relative positioning", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        #endregion
    }
}
