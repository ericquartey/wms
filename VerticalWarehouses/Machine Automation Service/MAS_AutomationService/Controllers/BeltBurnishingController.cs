using System;
using System.IO;
using System.Threading.Tasks;
using Ferretto.VW.Common_Utils.Messages.Data;
using Ferretto.VW.Common_Utils.Messages.Enumerations;
using Ferretto.VW.MAS_DataLayer.Enumerations;
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
    public class BeltBurnishingController : ControllerBase
    {
        #region Fields

        private readonly IDataLayerConfigurationValueManagment dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public BeltBurnishingController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IDataLayerConfigurationValueManagment)) as IDataLayerConfigurationValueManagment;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpPost("Execute/{upperBound}/{lowerBound}/{requiredCycles}")]
        public async Task ExecuteAsync(decimal upperBound, decimal lowerBound, int requiredCycles)
        {
            await this.ExecuteBeltBurnishing_MethodAsync(upperBound, lowerBound, requiredCycles);
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

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
        }

        private async Task ExecuteBeltBurnishing_MethodAsync(decimal upperBound, decimal lowerBound, int requiredCycles)
        {
            var maxSpeed = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);
            var acceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);
            var deceleration = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);
            var resolution = await this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValueAsync(
                (long)VerticalAxis.Resolution, (long)ConfigurationCategory.VerticalAxis);

            var positioningMessageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                upperBound,
                maxSpeed,
                acceleration,
                deceleration,
                requiredCycles,
                lowerBound,
                upperBound,
                resolution);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    positioningMessageData,
                    "Execute Belt Burnishing Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.Positioning));
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

        private async Task<ActionResult<int>> GetIntegerConfigurationParameter_MethodAsync(string category, string parameter)
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
