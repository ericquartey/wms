using System;
using System.IO;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Mvc;
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
        public bool SetBeltBurnishingCompletion()
        {
            return this.SetBeltBurnishingCompletion_Method();
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
                (long)VerticalAxis.MaxEmptySpeed, ConfigurationCategory.VerticalAxis);
            var acceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptyAcceleration, ConfigurationCategory.VerticalAxis);
            var deceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptyDeceleration, ConfigurationCategory.VerticalAxis);

            var positioningMessageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                MovementMode.BeltBurnishing,
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

        private ActionResult<decimal> GetDecimalConfigurationParameter_Method(string categoryString, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), categoryString, out var categoryId);
            var category = (ConfigurationCategory)categoryId;

            switch (category)
            {
                case ConfigurationCategory.VerticalAxis:

                    var parseSuccess = Enum.TryParse(typeof(VerticalAxis), parameter, out var verticalAxisParameterId);

                    if (parseSuccess)
                    {
                        decimal value1;

                        try
                        {
                            value1 = this.dataLayerConfigurationValueManagement
                                .GetDecimalConfigurationValue(
                                (long)verticalAxisParameterId,
                                category);
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
                        decimal value2;
                        try
                        {
                            value2 = this.dataLayerConfigurationValueManagement
                                .GetDecimalConfigurationValue(
                                (long)horizontalAxisParameterId,
                                category);
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
                        decimal value3;
                        try
                        {
                            value3 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                                (long)resolutionCalibrationParameterId,
                                category);
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
            }

            return 0;
        }

        private ActionResult<int> GetIntegerConfigurationParameter_Method(string categoryString, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), categoryString, out var categoryId);
            Enum.TryParse(typeof(BeltBurnishing), parameter, out var parameterId);
            var category = (ConfigurationCategory)categoryId;

            if (parameterId != null)
            {
                int value;

                try
                {
                    value = this.dataLayerConfigurationValueManagement
                        .GetIntegerConfigurationValue(
                        (long)parameterId,
                        category);
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

        private bool SetBeltBurnishingCompletion_Method()
        {
            var result = true;

            try
            {
                this.dataLayerConfigurationValueManagement
                    .SetBoolConfigurationValue(
                    (long)SetupStatus.BeltBurnishingDone,
                    ConfigurationCategory.SetupStatus,
                    true);
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
