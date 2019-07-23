using System;
using System.IO;
using System.Threading.Tasks;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS_DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Enumerations;
using Ferretto.VW.MAS_Utils.Events;
using Ferretto.VW.MAS_Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class BeltBurnishingController : ControllerBase
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public BeltBurnishingController(IEventAggregator eventAggregator, IServiceProvider services)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = services.GetService(typeof(IConfigurationValueManagmentDataLayer)) as IConfigurationValueManagmentDataLayer;
            this.logger = services.GetService(typeof(ILogger)) as ILogger;
        }

        #endregion

        #region Methods

        [HttpPost("Execute/{upperBound}/{lowerBound}/{requiredCycles}")]
        public void Execute(decimal upperBound, decimal lowerBound, int requiredCycles)
        {
            this.ExecuteBeltBurnishing_Method(upperBound, lowerBound, requiredCycles);
        }

        [ProducesResponseType(200, Type = typeof(decimal))]
        [ProducesResponseType(404)]
        [HttpGet("GetDecimalConfigurationParameter/{category}/{parameter}")]
        public ActionResult<decimal> GetDecimalConfigurationParameter(string category, string parameter)
        {
            return this.GetDecimalConfigurationParameter_Method(category, parameter);
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        public ActionResult<int> GetIntegerConfigurationParameter(string category, string parameter)
        {
            return this.GetIntegerConfigurationParameter_Method(category, parameter);
        }

        [HttpPut("SetBeltBurnishingCompletion")]
        public async Task<bool> SetBeltBurnishingCompletionAsync()
        {
            return await this.SetBeltBurnishingCompletion_MethodAsync();
        }

        [ProducesResponseType(200)]
        [HttpGet("Stop")]
        public void Stop()
        {
            this.Stop_Method();
        }

        private void ExecuteBeltBurnishing_Method(decimal upperBound, decimal lowerBound, int requiredCycles)
        {
            var maxSpeed = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxSpeed, (long)ConfigurationCategory.VerticalAxis);
            var acceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxAcceleration, (long)ConfigurationCategory.VerticalAxis);
            var deceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxDeceleration, (long)ConfigurationCategory.VerticalAxis);

            var positioningMessageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                upperBound,
                maxSpeed,
                acceleration,
                deceleration,
                requiredCycles,
                lowerBound,
                upperBound);

            this.eventAggregator.GetEvent<CommandEvent>().Publish(
                new CommandMessage(
                    positioningMessageData,
                    "Execute Belt Burnishing Command",
                    MessageActor.FiniteStateMachines,
                    MessageActor.WebApi,
                    MessageType.Positioning));
        }

        private ActionResult<decimal> GetDecimalConfigurationParameter_Method(string category, string parameter)
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
                            value1 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue((long)verticalAxisParameterId, (long)categoryId);
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
                            value2 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue((long)horizontalAxisParameterId, (long)categoryId);
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
                            value3 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue((long)resolutionCalibrationParameterId, (long)categoryId);
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

        private ActionResult<int> GetIntegerConfigurationParameter_Method(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            Enum.TryParse(typeof(BeltBurnishing), parameter, out var parameterId);

            if (parameterId != null)
            {
                int value;

                try
                {
                    value = this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValue((long)parameterId, (long)categoryId);
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

        private async Task<bool> SetBeltBurnishingCompletion_MethodAsync()
        {
            var result = true;

            try
            {
                await this.dataLayerConfigurationValueManagement.SetBoolConfigurationValueAsync((long)SetupStatus.BeltBurnishingDone, (long)ConfigurationCategory.SetupStatus, true);
            }
            catch
            {
                result = false;
            }

            return result;
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
