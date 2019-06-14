using System;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.Common_Utils.Messages;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    public partial class TestController
    {
        #region Methods

        private void BayNowServiceableMethod()
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null, "Test bay now serviceable", MessageActor.MissionsManager, MessageActor.WebApi, MessageType.MissionCompleted, MessageStatus.OperationEnd));
        }

        private async Task ExecuteHomingMethod()
        {
            this.eventAggregator.GetEvent<NotificationEvent>()
                            .Publish(new NotificationMessage(null, "Homing Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.Homing, MessageStatus.OperationStart));
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

        private async Task ExecuteResolutionCalibrationMethod(decimal readInitialPosition, decimal readFinalPosition)
        {
            var resolutionCalibrationMessageData = new ResolutionCalibrationMessageData(readInitialPosition, readFinalPosition);
            var notificationMessage = new NotificationMessage(resolutionCalibrationMessageData, "Resolution Calibration Started", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.ResolutionCalibration, MessageStatus.OperationStart);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(notificationMessage);
            await Task.Delay(2000);
            resolutionCalibrationMessageData.Resolution = 1.0001m;
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(resolutionCalibrationMessageData,
                "Resolution Calibration Ended", MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.ResolutionCalibration, MessageStatus.OperationEnd));
        }

        private async Task ExecuteShutterPositioningMovementMethod()
        {
            var speedRate = 1.2m;
            var dto = new ShutterPositioningMovementMessageDataDTO(ShutterMovementDirection.Up, 1);
            dto.ShutterType = ShutterType.NoType;
            var dataInterface = new ShutterPositioningMessageData(ShutterPosition.Opened, dto.ShutterPositionMovement, dto.ShutterType, dto.BayNumber, speedRate);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(dataInterface, "Shutter Positioning Started",
                 MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.ShutterPositioning,
                MessageStatus.OperationStart));

            await Task.Delay(2000);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(dataInterface, "Shutter Positioning Completed",
                MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.ShutterPositioning,
                MessageStatus.OperationEnd));
        }

        private void ExecuteStopHomingMethod()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Stop Homing",
                            MessageActor.FiniteStateMachines, MessageActor.AutomationService, MessageType.Stop,
                            MessageVerbosity.Info));
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

        private ActionResult<int> GetIntegerConfigurationParameterMethod(string category, string parameter)
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

        private void HomingMethod()
        {
            var messageData = new HomingMessageData(Axis.Both);
            var message = new CommandMessage(messageData, "Homing", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Homing);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        private void HorizontalPositioningMethod()
        {
            var messageData = new PositioningMessageData(Axis.Horizontal, MovementType.Relative, 4096m, 200m, 200m, 200m, 0, 0, 0, 0, ResolutionCalibrationSteps.None);
            var message = new CommandMessage(messageData, "Horizontal relative positioning", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        private void StartShutterControlErrorMethod(int delay, int numberCycles)
        {
            var dataInterface = new ShutterControlMessageData(delay, numberCycles);

            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(dataInterface,
                "Simulated Shutter Error",
                 MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.ShutterControl,
                 MessageStatus.OperationError));
        }

        private async Task StartShutterControlMethod()
        {
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null, "Shutter Started",
                             MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.ShutterControl,
                            MessageStatus.OperationStart));
            await Task.Delay(2000);
            this.eventAggregator.GetEvent<NotificationEvent>().Publish(new NotificationMessage(null, "Shutter Completed",
                MessageActor.AutomationService, MessageActor.FiniteStateMachines, MessageType.ShutterControl,
                MessageStatus.OperationEnd));
        }

        private void StopFiniteStateMachineMethod()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Stop Homing",
                            MessageActor.FiniteStateMachines, MessageActor.AutomationService, MessageType.Stop,
                            MessageVerbosity.Info));
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
            var messageData = new PositioningMessageData(Axis.Vertical, MovementType.Relative, 4096m, 200m, 200m, 200m, 0, 0, 0, 0, ResolutionCalibrationSteps.None);
            var message = new CommandMessage(messageData, "Vertical relative positioning", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(message);
        }

        #endregion
    }
}
