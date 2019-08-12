using System;
using System.IO;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.Utils.Events;
using Ferretto.VW.MAS.Utils.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Prism.Events;
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

        private readonly IOffsetCalibrationDataLayer offsetCalibration;

        private readonly IVerticalAxisDataLayer verticalAxis;

        #endregion

        #region Constructors

        public VerticalOffsetController(
            IEventAggregator eventAggregator,
            IConfigurationValueManagmentDataLayer configurationValueManagmentDataLayer,
            ICellManagmentDataLayer dataLayerCellsManagement,
            IVerticalAxisDataLayer verticalAxisDataLayer,
            IOffsetCalibrationDataLayer offsetCalibrationDataLayer)
        {
            if (eventAggregator == null)
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            if (configurationValueManagmentDataLayer == null)
            {
                throw new ArgumentNullException(nameof(configurationValueManagmentDataLayer));
            }

            if (dataLayerCellsManagement == null)
            {
                throw new ArgumentNullException(nameof(dataLayerCellsManagement));
            }

            if (verticalAxisDataLayer == null)
            {
                throw new ArgumentNullException(nameof(verticalAxisDataLayer));
            }

            if (offsetCalibrationDataLayer == null)
            {
                throw new ArgumentNullException(nameof(offsetCalibrationDataLayer));
            }

            this.eventAggregator = eventAggregator;
            this.dataLayerConfigurationValueManagement = configurationValueManagmentDataLayer;
            this.dataLayerCellsManagement = dataLayerCellsManagement;
            this.verticalAxis = verticalAxisDataLayer;
            this.offsetCalibration = offsetCalibrationDataLayer;
        }

        #endregion

        #region Methods

        [HttpPost("mark-as-completed")]
        public IActionResult MarkAsCompleted()
        {
            try
            {
                this.dataLayerConfigurationValueManagement
                    .SetBoolConfigurationValue(
                    (long)SetupStatus.VerticalOffsetDone,
                    ConfigurationCategory.SetupStatus,
                    true);
            }
            catch (Exception)
            {
                this.UnprocessableEntity();
            }

            return this.Ok();
        }

        [HttpPost("ExecutePositioning/{targetPosition}")]
        public IActionResult ExecutePositioning(decimal targetPosition)
        {
            // TODO range check on targetPosition?

            var speed = this.verticalAxis.MaxEmptySpeed * this.offsetCalibration.FeedRateOC;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                MovementMode.Position,
                targetPosition,
                speed,
                this.verticalAxis.MaxEmptyAcceleration,
                this.verticalAxis.MaxEmptyDeceleration,
                0,
                0,
                0);

            var commandMessage = new CommandMessage(
                messageData,
                "Offset Calibration Start",
                MessageActor.FiniteStateMachines,
                MessageActor.WebApi,
                MessageType.Positioning);

            this.eventAggregator
                .GetEvent<CommandEvent>()
                .Publish(commandMessage);

            return this.Ok();
        }

        [HttpPost("ExecuteStepDown")]
        public void ExecuteStepDown()
        {
            var stepValue = this.offsetCalibration.StepValue;

            this.ExecuteStep(-stepValue);
        }

        [HttpPost("ExecuteStepUp")]
        public void ExecuteStepUp()
        {
            var stepValue = this.offsetCalibration.StepValue;

            this.ExecuteStep(stepValue);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [HttpGet("GetDecimalConfigurationParameter/{category}/{parameter}")]
        public ActionResult<decimal> GetDecimalConfigurationParameter(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            var categoryEnum = (ConfigurationCategory)categoryId;

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
                                categoryEnum);
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
                            value2 = this.dataLayerConfigurationValueManagement.GetDecimalConfigurationValue((long)offsetCalibrationParameterId, categoryEnum);
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
                    return this.BadRequest("Unexpected parameter category");
            }
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        public ActionResult<int> GetIntegerConfigurationParameter(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            Enum.TryParse(typeof(OffsetCalibration), parameter, out var parameterId);

            var categoryEnum = (ConfigurationCategory)categoryId;

            if (parameterId != null)
            {
                int value;

                try
                {
                    value = this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValue((long)parameterId, categoryEnum);
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

        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [HttpGet("GetLoadingUnitPositionParameter/{referenceCell}")]
        public ActionResult<decimal> GetLoadingUnitPositionParameter(int referenceCell)
        {
            if (referenceCell <= 0)
            {
                return this.BadRequest("Reference cell index cannot be negative or zero.");
            }
  
            try
            {
                var value = this.dataLayerCellsManagement.GetLoadingUnitPosition(referenceCell);

                return this.Ok(value.LoadingUnitCoord);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
            {
                return this.NotFound("Parameter not found");
            }
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(404)]
        [HttpGet("GetLoadingUnitSideParameter/{category}/{parameter}")]
        public ActionResult<int> GetLoadingUnitSideParameter(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            Enum.TryParse(typeof(OffsetCalibration), parameter, out var parameterId);

            if (parameterId == null)
            {
                return this.BadRequest("Specified parameter does not exist.");
            }
            
            LoadingUnitPosition value;
            var cellId = this.dataLayerConfigurationValueManagement.GetIntegerConfigurationValue((long)OffsetCalibration.ReferenceCell, ConfigurationCategory.OffsetCalibration);

            try
            {
                value = this.dataLayerCellsManagement.GetLoadingUnitPosition(cellId);

                return this.Ok((int)value.LoadingUnitSide);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
            {
                return this.NotFound("Parameter not found");
            }
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost]
        public IActionResult Set(decimal newOffset)
        {
            try
            {
                this.dataLayerConfigurationValueManagement
                    .SetDecimalConfigurationValue(
                        (long)VerticalAxis.Offset,
                        ConfigurationCategory.VerticalAxis,
                        newOffset);
            }
            catch (Exception)
            {
                return this.UnprocessableEntity();
            }

            return this.Ok();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet("Stop")]
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
        

        private void ExecuteStep(decimal displacement)
        {
            var maxSpeed = this.verticalAxis.MaxEmptySpeed;
            var maxAcceleration = this.verticalAxis.MaxEmptyAcceleration;
            var maxDeceleration = this.verticalAxis.MaxEmptyDeceleration;
            var feedRate = this.offsetCalibration.FeedRateOC;

            var speed = maxSpeed * feedRate;

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                MovementMode.Position,
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

        #endregion
    }
}
