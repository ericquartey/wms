using System;
using System.IO;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;
using Microsoft.AspNetCore.Http;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResolutionCalibrationProcedureController : BaseAutomationController
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

        private readonly IResolutionCalibrationDataLayer resolutionCalibration;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalAxisDataLayer verticalAxis;

        #endregion

        #region Constructors

        public ResolutionCalibrationProcedureController(
            IEventAggregator eventAggregator,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            IResolutionCalibrationDataLayer resolutionCalibration,
            IVerticalAxisDataLayer verticalAxisDataLayer,
            ISetupStatusProvider setupStatusProvider)
            : base(eventAggregator)
        {
            if (dataLayerConfigurationValueManagement == null)
            {
                throw new ArgumentNullException(nameof(dataLayerConfigurationValueManagement));
            }

            if (resolutionCalibration == null)
            {
                throw new ArgumentNullException(nameof(resolutionCalibration));
            }

            if (verticalAxisDataLayer == null)
            {
                throw new ArgumentNullException(nameof(verticalAxisDataLayer));
            }

            if (setupStatusProvider == null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            this.verticalAxis = verticalAxisDataLayer;
            this.setupStatusProvider = setupStatusProvider;
            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.resolutionCalibration = resolutionCalibration;
        }

        #endregion

        #region Methods

        [HttpGet("computed-resolution")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult<decimal> GetComputedResolution(decimal readDistance, decimal desiredInitialPosition, decimal desiredFinalPosition, decimal resolution)
        {
            if (desiredFinalPosition == desiredInitialPosition)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Detail = "Initial and final position values cannot be the same."
                    });
            }

            // TODO: Is it better to compute the calculus inside the FSM ??

            var desideredDistance = desiredFinalPosition - desiredInitialPosition;

            return resolution * readDistance / desideredDistance;
        }

        [HttpGet("decimal-configuration-parameter/{category}/{parameter}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<decimal> GetDecimalConfigurationParameterAsync(string category, string parameter)
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

                case ConfigurationCategory.ResolutionCalibration:
                    Enum.TryParse(typeof(ResolutionCalibration), parameter, out var resolutionCalibrationParameterId);
                    if (resolutionCalibrationParameterId != null)
                    {
                        decimal value3 = 0;
                        try
                        {
                            value3 = this.dataLayerConfigurationValueManagement
                                .GetDecimalConfigurationValue
                                ((long)resolutionCalibrationParameterId,
                                categoryEnum);
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

        [HttpPost("mark-as-completed")]
        public IActionResult MarkAsCompleted()
        {
            this.setupStatusProvider.CompleteVerticalResolution();

            return this.Ok();
        }

        [HttpPost("resolution")]
        public IActionResult SetResolutionParameter(decimal value)
        {
            this.verticalAxis.Resolution = value;

            return this.Ok();
        }

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult Start(decimal position, ResolutionCalibrationStep resolutionCalibrationStep)
        {
            var setupStatus = this.setupStatusProvider.Get();
            if (!setupStatus.VerticalResolution.CanBePerformed)
            {
                return this.UnprocessableEntity(
                    new ProblemDetails
                    {
                        Detail = "Resolution calibration procedure cannot be started if the 'vertical origin calibration' and 'belt burnishing' procedures are not completed."
                    });
            }

            var description = GetStepDescription(resolutionCalibrationStep);

            var maxSpeed = this.verticalAxis.MaxEmptySpeed;
            var feedRate = this.resolutionCalibration.FeedRate;

            var speed = maxSpeed * feedRate;
            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                MovementMode.Position,
                position,
                speed,
                this.verticalAxis.MaxEmptyAcceleration,
                this.verticalAxis.MaxEmptyDeceleration,
                0,
                0,
                0);

            this.PublishCommand(
                messageData,
                description,
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);

            return this.Accepted();
        }

        [HttpPost("stop")]
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

        private static string GetStepDescription(ResolutionCalibrationStep resolutionCalibrationStep)
        {
            string message;
            switch (resolutionCalibrationStep)
            {
                case ResolutionCalibrationStep.StartProcedure:
                    message = "Resolution Calibration Start";
                    break;

                case ResolutionCalibrationStep.InitialPosition:
                    message = "Resolution Calibration go to initial position";
                    break;

                case ResolutionCalibrationStep.Move:
                    message = "Resolution Calibration move to final position";
                    break;

                default:
                    message = string.Empty;
                    break;
            }

            return message;
        }

        #endregion
    }
}
