using System;
using System.IO;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
// ReSharper disable ArrangeThisQualifier

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VerticalOffsetProcedureController : BaseAutomationController
    {
        #region Fields

        private readonly ICellManagmentDataLayer dataLayerCellsManagement;

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IOffsetCalibrationDataLayer offsetCalibration;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalAxisDataLayer verticalAxis;

        #endregion

        #region Constructors

        public VerticalOffsetProcedureController(
            IEventAggregator eventAggregator,
            IConfigurationValueManagmentDataLayer configurationValueManagmentDataLayer,
            ICellManagmentDataLayer dataLayerCellsManagement,
            IVerticalAxisDataLayer verticalAxisDataLayer,
            IOffsetCalibrationDataLayer offsetCalibrationDataLayer,
            ISetupStatusProvider setupStatusProvider)
            : base(eventAggregator)
        {
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

            if (setupStatusProvider == null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            this.dataLayerConfigurationValueManagement = configurationValueManagmentDataLayer;
            this.dataLayerCellsManagement = dataLayerCellsManagement;
            this.verticalAxis = verticalAxisDataLayer;
            this.offsetCalibration = offsetCalibrationDataLayer;
            this.setupStatusProvider = setupStatusProvider;
        }

        #endregion

        #region Methods

        [HttpPost("ExecuteStepDown")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult ExecuteStepDown()
        {
            var stepValue = this.offsetCalibration.StepValue;

            this.ExecuteStep(-stepValue);

            return this.Accepted();
        }

        [HttpPost("ExecuteStepUp")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult ExecuteStepUp()
        {
            var stepValue = this.offsetCalibration.StepValue;

            this.ExecuteStep(stepValue);

            return this.Accepted();
        }

        [HttpGet("GetDecimalConfigurationParameter/{category}/{parameter}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
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

        [HttpGet("GetIntegerConfigurationParameter/{category}/{parameter}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
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

        [HttpGet("GetLoadingUnitPositionParameter/{referenceCell}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
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

        [HttpGet("GetLoadingUnitSideParameter/{category}/{parameter}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
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

        [HttpPost("mark-as-completed")]
        public IActionResult MarkAsCompleted()
        {
            this.setupStatusProvider.CompleteVerticalOffset();

            return this.Ok();
        }

        [HttpPost]
        public IActionResult Set(decimal newOffset)
        {
            this.dataLayerConfigurationValueManagement
                .SetDecimalConfigurationValue(
                    (long)VerticalAxis.Offset,
                    ConfigurationCategory.VerticalAxis,
                    newOffset);

            return this.Ok();
        }

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Start(decimal targetPosition)
        {
            // TODO range check on targetPosition

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

            this.PublishCommand(
                messageData,
                "Offset Calibration Start",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);

            return this.Accepted();
        }

        [HttpGet("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.PublishCommand(
                null,
                "Stop Command",
                MessageActor.FiniteStateMachines,
                MessageType.Stop);

            return this.Accepted();
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

            this.PublishCommand(
                messageData,
                "Offset Calibration Start",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);
        }

        #endregion
    }
}
