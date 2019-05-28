using System;
using System.IO;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.DTOs;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.Common_Utils.Messages.Interfaces;
using Ferretto.VW.MAS_DataLayer.Enumerations;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstallationController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

        private readonly ISetupStatus dataLayerSetupStatus;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public InstallationController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IDataLayerConfigurationValueManagment)) as IDataLayerConfigurationValueManagment;
            this.dataLayerSetupStatus = services.GetService(typeof(ISetupStatus)) as ISetupStatus;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpPost]
        [Route("ExecuteBeltBurnishing/{upperBound}/{lowerBound}/{requiredCycles}")]
        public async Task ExecuteBeltBurnishing(decimal upperBound, decimal lowerBound, int requiredCycles)
        {
            var speed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);
            var acceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);
            var deceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);
            var resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

            IVerticalPositioningMessageData verticalPositioningMessageData = new VerticalPositioningMessageData(Axis.Vertical, MovementType.Relative, upperBound,
                speed, acceleration, deceleration, requiredCycles, lowerBound, upperBound, resolution);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(verticalPositioningMessageData, "Execute Belt Burninshing Command",
                MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Positioning));
        }

        [HttpGet("ExecuteHoming")]
        public void ExecuteHoming()
        {
            IHomingMessageData homingData = new HomingMessageData(Axis.Both);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(homingData, "Execute Homing Command", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Homing));
        }

        [HttpPost]
        [Route("ExecuteMovement")]
        public async Task ExecuteMovement([FromBody]MovementMessageDataDTO data)
        {
            decimal maxSpeed = 0;
            decimal maxAcceleration = 0;
            decimal maxDeceleration = 0;
            decimal feedRate = 0;
            decimal initialTargetPosition = 0;
            decimal resolution = 0;

            try
            {
                var MachineDone = await this.dataLayerConfigurationValueManagement.GetBoolConfigurationValueAsync((long)SetupStatus.MachineDone, (long)ConfigurationCategory.SetupStatus);

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

                        if (MachineDone)
                            initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalManualMovements.RecoveryTargetPosition,
                                (long)ConfigurationCategory.VerticalManualMovements);
                        else
                            initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalManualMovements.InitialTargetPosition,
                                (long)ConfigurationCategory.VerticalManualMovements);

                        // INFO +1 for Up, -1 for Down
                        initialTargetPosition *= data.Displacement;
                        resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)VerticalAxis.Resolution,
                            (long)ConfigurationCategory.VerticalAxis);

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

                        if (MachineDone)
                            initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)HorizontalManualMovements.RecoveryTargetPosition,
                                (long)ConfigurationCategory.HorizontalManualMovements);
                        else
                            initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)HorizontalManualMovements.InitialTargetPosition,
                                (long)ConfigurationCategory.HorizontalManualMovements);

                        initialTargetPosition = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)HorizontalManualMovements.InitialTargetPosition,
                            (long)ConfigurationCategory.HorizontalManualMovements);
                        // INFO +1 for Forward, -1 for Back
                        initialTargetPosition *= data.Displacement;
                        resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)HorizontalAxis.Resolution,
                            (long)ConfigurationCategory.HorizontalAxis);

                        break;
                }

                var speed = maxSpeed * feedRate;

                var messageData = new VerticalPositioningMessageData(data.Axis, data.MovementType, initialTargetPosition, speed, maxAcceleration, maxDeceleration, 0, 0, 0, resolution);
                this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(messageData, $"Execute {data.Axis} Positioning Command",
                    MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Positioning));
            }
            catch (Exception ex)
            {
                // TODO
            }
        }

        [HttpPost]
        [Route("ExecuteResolutionCalibration/{readInitialPosition}/{readFinalPosition}")]
        public void ExecuteResolutionCalibration(decimal readInitialPosition, decimal readFinalPosition)
        {
            var resolutionCalibrationMessageData = new ResolutionCalibrationMessageData(readInitialPosition, readFinalPosition);
            var commandMessage = new CommandMessage(resolutionCalibrationMessageData, "Resolution Calibration Start", MessageActor.FiniteStateMachines,
                MessageActor.WebApi, MessageType.ResolutionCalibration);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(commandMessage);
        }

        [HttpGet("ExecuteSensorsChangedCommand")]
        public void ExecuteSensorsChangedCommand()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Sensors changed Command", MessageActor.FiniteStateMachines,
                MessageActor.WebApi, MessageType.SensorsChanged));
        }

        [HttpPost]
        [Route("ExecuteShutterPositioningMovement")]
        public async Task ExecuteShutterPositioningMovementAsync([FromBody]ShutterPositioningMovementMessageDataDTO data)
        {
            switch (data.BayNumber)
            {
                case 1:
                    data.ShutterType = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)GeneralInfo.Shutter1Type, (long)ConfigurationCategory.GeneralInfo);
                    break;

                case 2:
                    data.ShutterType = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)GeneralInfo.Shutter2Type, (long)ConfigurationCategory.GeneralInfo);
                    break;

                case 3:
                    data.ShutterType = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)GeneralInfo.Shutter3Type, (long)ConfigurationCategory.GeneralInfo);
                    break;
            }

            var messageData = new ShutterPositioningMessageData(data.ShutterPositionMovement);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(messageData, "Execute Shutter Positioning Movement Command", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.ShutterPositioning));
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("GetDecimalConfigurationParameter/{category}/{parameter}")]
        public async Task<ActionResult<decimal>> GetDecimalConfigurationParameterAsync(string category, string parameter)
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

        [ProducesResponseType(200, Type = typeof(bool[]))]
        [ProducesResponseType(500)]
        [HttpGet("GetInstallationStatus")]
        public async Task<ActionResult<bool[]>> GetInstallationStatus()
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

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        public async Task<ActionResult<int>> GetIntegerConfigurationParameterAsync(string category, string parameter)
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

        [HttpPost]
        [Route("LSM-HorizontalAxis/{Displacement}/{Axis}/{MovementType}/{SpeedPercentage}")]
        public async Task HorizontalAxisForLSM(decimal? displacement, Axis axis, MovementType movementType, uint speedPercentage = 100)
        {
            //TODO: I temporary used IMovementMessageData for getting the relevant parameters. This interface is going to be modified in the future, so we need to use the modified interface.
            IMovementMessageData horizontalAxisForLSM = new MovementMessageData(displacement, axis, movementType, speedPercentage);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(horizontalAxisForLSM, "LSM Horizontal Axis Movements", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Movement));
        }

        [HttpPost]
        [Route("LSM-ShutterPositioning/{shutterMovementDirection}")]
        public async Task ShutterPositioningForLSM(ShutterMovementDirection shutterMovementDirection)
        {
            IShutterPositioningMessageData shutterPositioningForLSM = new ShutterPositioningMessageData(shutterMovementDirection);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(shutterPositioningForLSM, "LSM Shutter Movements", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.ShutterPositioning));
        }

        [HttpGet("StartShutterControl/{delay}/{numberCycles}")]
        public async Task StartShutterControlAsync(int delay, int numberCycles)
        {
            IShutterControlMessageData shutterControlMessageData = new ShutterControlMessageData(delay, numberCycles);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(shutterControlMessageData, "Shutter Started", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.ShutterControl));
        }

        [ProducesResponseType(200)]
        [HttpGet("StopCommand")]
        public void StopCommand()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(null, "Stop Command", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Stop));
            this.Ok();
        }

        [HttpPost]
        [Route("LSM-VerticalAxis/{Displacement}/{Axis}/{MovementType}/{SpeedPercentage}")]
        public async Task VerticalAxisForLSM(decimal? displacement, Axis axis, MovementType movementType, uint speedPercentage = 100)
        {
            //TODO: I temporary used IMovementMessageData for getting the relevant parameters. This interface is going to be modified in the future, so we need to use the modified interface.
            IMovementMessageData verticalAxisForLSM = new MovementMessageData(displacement, axis, movementType, speedPercentage);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(new CommandMessage(verticalAxisForLSM, "LSM Vertical Axis Movements", MessageActor.FiniteStateMachines, MessageActor.WebApi, MessageType.Movement));
        }

        #endregion
    }
}
