using System;
using System.IO;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    public partial class InstallationController
    {
        #region Methods

        private async Task ExecuteBeltBurnishingMethod(decimal upperBound, decimal lowerBound, int requiredCycles)
        {
            var maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);
            var acceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);
            var deceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);
            var resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

            IPositioningMessageData positioningMessageData = new PositioningMessageData(Axis.Vertical, MovementType.Relative, upperBound,
                maxSpeed, acceleration, deceleration, requiredCycles, lowerBound, upperBound, resolution, ResolutionCalibrationSteps.None);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(positioningMessageData, "Execute Belt Burninshing Command",
                MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Positioning));
        }

        private void ExecuteHomingMethod()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.Both);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(homingData, "Execute Homing Command", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Homing));
        }

        private async Task ExecuteMovementMethod(MovementMessageDataDTO data)
        {
            decimal maxSpeed = 0;
            decimal maxAcceleration = 0;
            decimal maxDeceleration = 0;
            decimal feedRate = 0;
            decimal initialTargetPosition = 0;
            decimal resolution = 0;

            try
            {
                var machineDone = await this.dataLayerConfigurationValueManagement.GetBoolConfigurationValueAsync((long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus);

                switch (data.Axis)
                {
                    // INFO Vertical LSM
                    case Axis.Vertical:
                        maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);
                        maxAcceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxAcceleration,
                            (long)ConfigurationCategory.VerticalAxis);
                        maxDeceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxDeceleration,
                            (long)ConfigurationCategory.VerticalAxis);
                        feedRate = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalManualMovements.FeedRate,
                            (long)ConfigurationCategory.VerticalManualMovements);

                        if (machineDone)
                        {
                            initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                                (long)VerticalManualMovements.RecoveryTargetPosition, (long)ConfigurationCategory.VerticalManualMovements);
                        }
                        else
                        {
                            initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                                (long)VerticalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.VerticalManualMovements);
                        }

                        // INFO +1 for Up, -1 for Down
                        initialTargetPosition *= data.Displacement;
                        resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                            (long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

                        break;

                    // INFO Horizontal LSM
                    case Axis.Horizontal:
                        maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)HorizontalAxis.MaxSpeed, (long)ConfigurationCategory.HorizontalAxis);
                        maxAcceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)HorizontalAxis.MaxAcceleration,
                            (long)ConfigurationCategory.HorizontalAxis);
                        maxDeceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)HorizontalAxis.MaxDeceleration,
                            (long)ConfigurationCategory.HorizontalAxis);
                        feedRate = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)HorizontalManualMovements.FeedRate,
                            (long)ConfigurationCategory.HorizontalManualMovements);

                        if (machineDone)
                        {
                            initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                                (long)HorizontalManualMovements.RecoveryTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);
                        }
                        else
                        {
                            initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                                (long)HorizontalManualMovements.InitialTargetPosition, (long)ConfigurationCategory.HorizontalManualMovements);
                        }

                        initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)HorizontalManualMovements.InitialTargetPosition,
                            (long)ConfigurationCategory.HorizontalManualMovements);

                        // INFO +1 for Forward, -1 for Back
                        initialTargetPosition *= data.Displacement;
                        resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)HorizontalAxis.Resolution,
                            (long)ConfigurationCategory.HorizontalAxis);

                        break;
                }

                var speed = maxSpeed * feedRate;

                var messageData = new PositioningMessageData(data.Axis, data.MovementType, initialTargetPosition, speed, maxAcceleration, maxDeceleration, 0, 0, 0, resolution, ResolutionCalibrationSteps.None);
                this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(messageData, $"Execute {data.Axis} Positioning Command",
                    MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Positioning));
            }
            catch (Exception ex)
            {
                // TODO
            }
        }

        private void ExecuteResolutionCalibrationMethod(decimal readInitialPosition, decimal readFinalPosition)
        {
            var resolutionCalibrationMessageData = new ResolutionCalibrationMessageData(readInitialPosition, readFinalPosition);
            var commandMessage = new CommandMessage(resolutionCalibrationMessageData, "Resolution Calibration Start", MessageActor.FiniteStateMachines,
                MessageActor.WebApi, MessageType.ResolutionCalibration);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(commandMessage);
        }

        private async Task ExecuteResolutionMethod(decimal position, ResolutionCalibrationSteps resolutionCalibrationSteps)
        {
            var maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);
            var maxAcceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);
            var maxDeceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);
            var feedRate = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalManualMovements.FeedRate, (long)ConfigurationCategory.VerticalManualMovements);

            var resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)HorizontalAxis.Resolution, (long)ConfigurationCategory.HorizontalAxis);

            var speed = maxSpeed * feedRate;

            var messageData = new PositioningMessageData(Axis.Vertical, MovementType.Absolute, position, speed, maxAcceleration, maxDeceleration, 0, 0, 0, resolution, resolutionCalibrationSteps);

            var commandMessage = new CommandMessage(messageData, "Resolution Calibration Start", MessageActor.FiniteStateMachines, MessageActor.WebApi,
                MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(commandMessage);
        }

        private void ExecuteSensorsChangedMethod()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Sensors changed Command", MessageActor.FiniteStateMachines,
                            MessageActor.WebApi, MessageType.SensorsChanged));
        }

        private async Task ExecuteShutterPositioningMovementMethod(ShutterPositioningMovementMessageDataDTO data)
        {
            switch (data.ShutterType)
            {
                case ShutterType.NoType:
                    await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)GeneralInfo.Shutter1Type, (long)ConfigurationCategory.GeneralInfo);
                    break;

                case ShutterType.Shutter2Type:
                    await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)GeneralInfo.Shutter2Type, (long)ConfigurationCategory.GeneralInfo);
                    break;

                case ShutterType.Shutter3Type:
                    await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)GeneralInfo.Shutter3Type, (long)ConfigurationCategory.GeneralInfo);
                    break;
            }

            //TODO Define Low Speed Movement shutter velocity Rate. SpeedRate needs to be multiplied by 100.
            var speedRate = 100m;

            var messageData = new ShutterPositioningMessageData(ShutterPosition.Closed, data.ShutterPositionMovement, ShutterType.Shutter3Type, data.BayNumber, speedRate);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(messageData, "Execute Shutter Positioning Movement Command", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.ShutterPositioning));
        }

        private async Task<ActionResult<decimal>> GetDecimalConfigurationParameterMethod(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);

            switch (categoryId)
            {
                case ConfigurationCategory.VerticalAxis:

                    Enum.TryParse(typeof(VerticalAxis), parameter, out var verticalAxisParameterId);

                    if (verticalAxisParameterId != null)
                    {
                        decimal value1 = 0;

                        try
                        {
                            value1 = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)verticalAxisParameterId, (long)categoryId);
                        }
                        catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)

                        {
                            return this.NotFound("Parameter not found");
                        }

                        return this.Ok(value1);
                    }
                    else
                    {
                        return this.NotFound("Parameter not found");
                    }

                case ConfigurationCategory.HorizontalAxis:

                    Enum.TryParse(typeof(HorizontalAxis), parameter, out var horizontalAxisParameterId);
                    if (horizontalAxisParameterId != null)
                    {
                        decimal value2 = 0;
                        try
                        {
                            value2 = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)horizontalAxisParameterId, (long)categoryId);
                        }
                        catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)

                        {
                            return this.NotFound("Parameter not found");
                        }

                        return this.Ok(value2);
                    }
                    else
                    {
                        return this.NotFound("Parameter not found");
                    }
                case ConfigurationCategory.ResolutionCalibration:
                    Enum.TryParse(typeof(ResolutionCalibration), parameter, out var resolutionCalibrationParameterId);
                    if (resolutionCalibrationParameterId != null)
                    {
                        decimal value3 = 0;
                        try
                        {
                            value3 = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)resolutionCalibrationParameterId, (long)categoryId);
                        }
                        catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)

                        {
                            return this.NotFound("Parameter not found");
                        }

                        return this.Ok(value3);
                    }
                    else
                    {
                        return this.NotFound("Parameter not found");
                    }

                default:
                    break;
            }

            return 0;
        }

        private async Task<ActionResult<bool[]>> GetInstallationStatusMethod()
        {
            var value = new bool[23];
            try
            {
                value[0] = await this.dataLayerSetupStatus.VerticalHomingDone;
                value[1] = await this.dataLayerSetupStatus.HorizontalHomingDone;
                value[2] = await this.dataLayerSetupStatus.BeltBurnishingDone;
                value[3] = await this.dataLayerSetupStatus.VerticalResolutionDone;
                value[4] = await this.dataLayerSetupStatus.VerticalOffsetDone;
                value[5] = await this.dataLayerSetupStatus.CellsControlDone;
                value[6] = await this.dataLayerSetupStatus.PanelsControlDone;
                value[7] = await this.dataLayerSetupStatus.Shape1Done;
                value[8] = await this.dataLayerSetupStatus.Shape2Done;
                value[9] = await this.dataLayerSetupStatus.Shape3Done;
                value[10] = await this.dataLayerSetupStatus.WeightMeasurementDone;
                value[11] = await this.dataLayerSetupStatus.Shutter1Done;
                value[12] = await this.dataLayerSetupStatus.Shutter2Done;
                value[13] = await this.dataLayerSetupStatus.Shutter3Done;
                value[14] = await this.dataLayerSetupStatus.Bay1ControlDone;
                value[15] = await this.dataLayerSetupStatus.Bay2ControlDone;
                value[16] = await this.dataLayerSetupStatus.Bay3ControlDone;
                value[17] = await this.dataLayerSetupStatus.FirstDrawerLoadDone;
                value[18] = await this.dataLayerSetupStatus.DrawersLoadedDone;
                value[19] = await this.dataLayerSetupStatus.Laser1Done;
                value[20] = await this.dataLayerSetupStatus.Laser2Done;
                value[21] = await this.dataLayerSetupStatus.Laser3Done;
                value[22] = await this.dataLayerSetupStatus.MachineDone;
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
            {
                return this.NotFound("Setup configuration not found");
            }

            return this.Ok(value);
        }

        private async Task<ActionResult<int>> GetIntegerConfigurationParameterMethod(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            Enum.TryParse(typeof(BeltBurnishing), parameter, out var parameterId);

            if (parameterId != null)
            {
                int value;

                try
                {
                    value = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)parameterId, (long)categoryId);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
                {
                    return this.NotFound("Parameter not found");
                }

                return this.Ok(value);
            }
            else
            {
                return this.NotFound("Parameter not found");
            }
        }

        private void HorizontalAxisForLSMMethod(decimal? displacement, Axis axis, MovementType movementType, uint speedPercentage)
        {
            IMovementMessageData horizontalAxisForLSM = new MovementMessageData(displacement, axis, movementType, speedPercentage);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(horizontalAxisForLSM, "LSM Horizontal Axis Movements", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Movement));
        }

        private void ShutterPositioningForLSMMethod(int bayNumber, decimal speedRate)
        {
            IShutterPositioningMessageData shutterPositioningForLSM = new ShutterPositioningMessageData(ShutterPosition.Closed, ShutterMovementDirection.Down, ShutterType.Shutter3Type, bayNumber, speedRate);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(shutterPositioningForLSM, "LSM Shutter Movements", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.ShutterPositioning));
        }

        private void StartShutterControlMethod(int delay, int numberCycles)
        {
            IShutterControlMessageData shutterControlMessageData = new ShutterControlMessageData(delay, numberCycles);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(shutterControlMessageData, "Shutter Started", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.ShutterControl));
        }

        private void StopCommandMethod()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Stop Command", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Stop));
            this.Ok();
        }

        private void VerticalAxisForLSMMethod(decimal? displacement, Axis axis, MovementType movementType, uint speedPercentage)
        {
            IMovementMessageData verticalAxisForLSM = new MovementMessageData(displacement, axis, movementType, speedPercentage);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(verticalAxisForLSM, "LSM Vertical Axis Movements", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Movement));
        }

        #endregion
    }
}
