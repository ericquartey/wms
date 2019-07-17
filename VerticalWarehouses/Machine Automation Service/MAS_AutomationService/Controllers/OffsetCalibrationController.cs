using System;
using System.IO;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace Ferretto.VW.MAS_AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class OffsetCalibrationController : ControllerBase
    {
        #region Fields

        private readonly ICellManagmentDataLayer dataLayerCellsManagement;

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public OffsetCalibrationController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IConfigurationValueManagmentDataLayer)) as IConfigurationValueManagmentDataLayer;
            this.dataLayerCellsManagement = services.GetService(typeof(ICellManagmentDataLayer)) as ICellManagmentDataLayer;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpPost("ExecuteCompleted")]
        public async Task<bool> ExecuteCompletedAsync()
        {
            return await this.ExecuteCompleted_MethodAsync();
        }

        [HttpGet("ExecutePositioning")]
        public async Task ExecutePositioningAsync()
        {
            await this.ExecutePositioning_MethodAsync();
        }

        [HttpGet("ExecuteStepDown")]
        public async Task ExecuteStepDownAsync()
        {
            var stepValue = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)OffsetCalibration.StepValue, (long)ConfigurationCategory.OffsetCalibration);

            await this.ExecuteStep_MethodAsync(-stepValue);
        }

        [HttpGet("ExecuteStepUp")]
        public async Task ExecuteStepUpAsync()
        {
            var stepValue = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)OffsetCalibration.StepValue, (long)ConfigurationCategory.OffsetCalibration);

            await this.ExecuteStep_MethodAsync(stepValue);
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("GetDecimalConfigurationParameter/{category}/{parameter}")]
        public async Task<ActionResult<decimal>> GetDecimalConfigurationParameterAsync(string category, string parameter)
        {
            return await this.GetDecimalConfigurationParameter_MethodAsync(category, parameter);
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        public async Task<ActionResult<int>> GetIntegerConfigurationParameterAsync(string category, string parameter)
        {
            return await this.GetIntegerConfigurationParameter_MethodAsync(category, parameter);
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetLoadingUnitPositionParameter/{category}/{parameter}")]
        public async Task<ActionResult<decimal>> GetLoadingUnitPositionParameterAsync(string category, string parameter)
        {
            return await this.GetLoadingUnitPositionParameter_MethodAsync(category, parameter);
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetLoadingUnitSideParameter/{category}/{parameter}")]
        public async Task<ActionResult<int>> GetLoadingUnitSideParameterAsync(string category, string parameter)
        {
            return await this.GetLoadingUnitSideParameter_MethodAsync(category, parameter);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(422)]
        [ProducesResponseType(400)]
        [HttpPost("SetOffsetParameter/{newOffset}/")]
        public async Task<bool> SetOffsetParameterAsync(decimal newOffset)
        {
            return await this.SetOffsetParameter_MethodAsync(newOffset);
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
        }

        private async Task<bool> ExecuteCompleted_MethodAsync()
        {
            var completionPersist = true;

            try
            {
                await this.dataLayerConfigurationValueManagement.SetBoolConfigurationValueAsync((long)SetupStatus.VerticalOffsetDone, (long)ConfigurationCategory.SetupStatus, true);
            }
            catch (Exception)
            {
                completionPersist = false;
            }

            return completionPersist;
        }

        private async Task ExecutePositioning_MethodAsync()
        {
            var referenceCell = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync(
                (long)OffsetCalibration.ReferenceCell, (long)ConfigurationCategory.OffsetCalibration);

            var position = 10; //TODO Retrieve the position related to the cellReference value

            var maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxEmptySpeed, (long)ConfigurationCategory.VerticalAxis);
            var maxAcceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxEmptyAcceleration, (long)ConfigurationCategory.VerticalAxis);
            var maxDeceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxEmptyDeceleration, (long)ConfigurationCategory.VerticalAxis);
            var feedRate = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)OffsetCalibration.FeedRate, (long)ConfigurationCategory.OffsetCalibration);
            var resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

            var speed = maxSpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                position,
                speed,
                maxAcceleration,
                maxDeceleration,
                0,
                0,
                0,
                resolution);

            var commandMessage = new CommandMessage(
                messageData,
                "Offset Calibration Start",
                MessageActor.FiniteStateMachines,
                MessageActor.WebApi,
                MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(commandMessage);
        }

        private async Task ExecuteStep_MethodAsync(decimal displacement)
        {
            var maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxEmptySpeed, (long)ConfigurationCategory.VerticalAxis);
            var maxAcceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxEmptyAcceleration, (long)ConfigurationCategory.VerticalAxis);
            var maxDeceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxEmptyDeceleration, (long)ConfigurationCategory.VerticalAxis);
            var feedRate = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)OffsetCalibration.FeedRate, (long)ConfigurationCategory.OffsetCalibration);
            var resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

            var speed = maxSpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                displacement,
                speed,
                maxAcceleration,
                maxDeceleration,
                0,
                0,
                0,
                resolution);

            var commandMessage = new CommandMessage(
                messageData,
                "Offset Calibration Start",
                MessageActor.FiniteStateMachines,
                MessageActor.WebApi,
                MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(commandMessage);
        }

        private async Task<ActionResult<decimal>> GetDecimalConfigurationParameter_MethodAsync(string category, string parameter)
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

                case ConfigurationCategory.OffsetCalibration:
                    Enum.TryParse(typeof(OffsetCalibration), parameter, out var offsetCalibrationParameterId);
                    if (offsetCalibrationParameterId != null)
                    {
                        decimal value2 = 0;
                        try
                        {
                            value2 = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync((long)offsetCalibrationParameterId, (long)categoryId);
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

                default:
                    break;
            }

            return 0;
        }

        private async Task<ActionResult<int>> GetIntegerConfigurationParameter_MethodAsync(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            Enum.TryParse(typeof(OffsetCalibration), parameter, out var parameterId);

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

        private async Task<ActionResult<decimal>> GetLoadingUnitPositionParameter_MethodAsync(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            Enum.TryParse(typeof(OffsetCalibration), parameter, out var parameterId);

            if (parameterId != null)
            {
                LoadingUnitPosition value;
                var cellId = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)OffsetCalibration.ReferenceCell, (long)ConfigurationCategory.OffsetCalibration);

                try
                {
                    value = this.dataLayerCellsManagement.GetLoadingUnitPosition(cellId);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
                {
                    return this.NotFound("Parameter not found");
                }

                return this.Ok(value.LoadingUnitCoord);
            }
            else
            {
                return this.NotFound("Parameter not found");
            }
        }

        private async Task<ActionResult<int>> GetLoadingUnitSideParameter_MethodAsync(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            Enum.TryParse(typeof(OffsetCalibration), parameter, out var parameterId);

            if (parameterId != null)
            {
                LoadingUnitPosition value;
                var cellId = await this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValueAsync((long)OffsetCalibration.ReferenceCell, (long)ConfigurationCategory.OffsetCalibration);

                try
                {
                    value = this.dataLayerCellsManagement.GetLoadingUnitPosition(cellId);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
                {
                    return this.NotFound("Parameter not found");
                }

                return this.Ok((int)value.LoadingUnitSide);
            }
            else
            {
                return this.NotFound("Parameter not found");
            }
        }

        private async Task<bool> SetOffsetParameter_MethodAsync(decimal newOffset)
        {
            var resultAssignment = true;

            try
            {
                await this.dataLayerConfigurationValueManagement.SetDecimalConfigurationValueAsync((long)VerticalAxis.Offset, (long)ConfigurationCategory.VerticalAxis, newOffset);
            }
            catch (Exception)
            {
                resultAssignment = false;
            }

            return resultAssignment;
        }

        private void Stop_Method()
        {
            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    null,
                    "Stop Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.Stop));
        }

        #endregion
    }
}
