using System;
using System.IO;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Microsoft.AspNetCore.Http;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("1.0.0/Installation/[controller]")]
    [ApiController]
    public class VerticalOffsetController : ControllerBase
    {
        #region Fields

        private readonly ICellManagmentDataLayer dataLayerCellsManagement;

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IEventAggregator eventAggregator;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public VerticalOffsetController(
            IEventAggregator eventAggregator,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            ICellManagmentDataLayer dataLayerCellsManagement,
            ILogger<VerticalOffsetController> logger)
        {
            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.dataLayerCellsManagement = dataLayerCellsManagement;
            this.logger = logger;
        }

        #endregion

        #region Methods

        [HttpPost("ExecutePositioning")]
        public void ExecutePositioning()
        {
            this.ExecutePositioning_Method();
        }

        [HttpPost("ExecuteStepDown")]
        public void ExecuteStepDown()
        {
            var stepValue = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)OffsetCalibration.StepValue, ConfigurationCategory.OffsetCalibration);

            this.ExecuteStep_Method(-stepValue);
        }

        [HttpPost("ExecuteStepUp")]
        public void ExecuteStepUp()
        {
            var stepValue = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)OffsetCalibration.StepValue, ConfigurationCategory.OffsetCalibration);

            this.ExecuteStep_Method(stepValue);
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

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("reference-cell")]
        public ActionResult<LoadingUnitPosition> GetLoadingUnitSideParameter()
        {
            var cellId = this.dataLayerConfigurationValueManagement
                .GetIntegerConfigurationValue(
                    (long)OffsetCalibration.ReferenceCell,
                    ConfigurationCategory.OffsetCalibration);

            try
            {
                var loadingUnitPosition = this.dataLayerCellsManagement.GetLoadingUnitPosition(cellId);

                return this.Ok(loadingUnitPosition);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
            {
                return this.UnprocessableEntity("Parameter not found");
            }
        }

        [HttpPost("mark-as-complete")]
        public IActionResult MarkAsComplete()
        {
            this.dataLayerConfigurationValueManagement.SetBoolConfigurationValue(
                (long)SetupStatus.VerticalOffsetDone,
                ConfigurationCategory.SetupStatus,
                true);

            return this.Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost]
        public IActionResult Set(decimal offset)
        {
            this.dataLayerConfigurationValueManagement.SetDecimalConfigurationValue(
                (long)VerticalAxis.Offset,
                ConfigurationCategory.VerticalAxis,
                offset);

            return this.Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpPost("stop")]
        public IActionResult Stop()
        {
            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(
                    new CommandMessage(
                        null,
                        "Stop Command",
                        MessageActor.FiniteStateMachines,
                        MessageActor.WebApi,
                        MessageType.Stop));

            return this.Ok();
        }

        private void ExecutePositioning_Method()
        {
            var referenceCell = this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValue(
                (long)OffsetCalibration.ReferenceCell, ConfigurationCategory.OffsetCalibration);

            var position = 10; //TODO Retrieve the position related to the cellReference value

            var maxSpeed = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptySpeed, ConfigurationCategory.VerticalAxis);
            var maxAcceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptyAcceleration, ConfigurationCategory.VerticalAxis);
            var maxDeceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptyDeceleration, ConfigurationCategory.VerticalAxis);
            var feedRate = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)OffsetCalibration.FeedRate, ConfigurationCategory.OffsetCalibration);

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
                0);

            var commandMessage = new CommandMessage(
                messageData,
                "Offset Calibration Start",
                MessageActor.FiniteStateMachines,
                MessageActor.WebApi,
                MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(commandMessage);
        }

        private void ExecuteStep_Method(decimal displacement)
        {
            var maxSpeed = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptySpeed, ConfigurationCategory.VerticalAxis);
            var maxAcceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptyAcceleration, ConfigurationCategory.VerticalAxis);
            var maxDeceleration = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)VerticalAxis.MaxEmptyDeceleration, ConfigurationCategory.VerticalAxis);
            var feedRate = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue(
                (long)OffsetCalibration.FeedRate, ConfigurationCategory.OffsetCalibration);

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
                0);

            var commandMessage = new CommandMessage(
                messageData,
                "Offset Calibration Start",
                MessageActor.FiniteStateMachines,
                MessageActor.WebApi,
                MessageType.Positioning);
            this.eventAggregator.GetEvent<CommandEvent>().Publish(commandMessage);
        }

        private ActionResult<decimal> GetDecimalConfigurationParameter_Method(string categoryString, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), categoryString, out var categoryId);
            var category = (ConfigurationCategory)categoryId;

            switch (categoryId)
            {
                case ConfigurationCategory.VerticalAxis:

                    Enum.TryParse(typeof(VerticalAxis), parameter, out var verticalAxisParameterId);

                    if (verticalAxisParameterId != null)
                    {
                        decimal value1 = 0;

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

                case ConfigurationCategory.OffsetCalibration:
                    Enum.TryParse(typeof(OffsetCalibration), parameter, out var offsetCalibrationParameterId);
                    if (offsetCalibrationParameterId != null)
                    {
                        decimal value2 = 0;
                        try
                        {
                            value2 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue((long)offsetCalibrationParameterId, category);
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

        private ActionResult<int> GetIntegerConfigurationParameter_Method(string categoryString, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), categoryString, out var categoryId);
            Enum.TryParse(typeof(OffsetCalibration), parameter, out var parameterId);

            var category = (ConfigurationCategory)categoryId;

            if (parameterId != null)
            {
                int value;

                try
                {
                    value = this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValue((long)parameterId, category);
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

        #endregion
    }
}
