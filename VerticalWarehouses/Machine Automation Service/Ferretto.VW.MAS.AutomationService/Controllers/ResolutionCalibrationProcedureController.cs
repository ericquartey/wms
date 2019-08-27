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
using Ferretto.VW.MAS.AutomationService.Models;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResolutionCalibrationProcedureController : BaseAutomationController
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer configurationProvider;

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
            if (dataLayerConfigurationValueManagement is null)
            {
                throw new ArgumentNullException(nameof(dataLayerConfigurationValueManagement));
            }

            if (resolutionCalibration is null)
            {
                throw new ArgumentNullException(nameof(resolutionCalibration));
            }

            if (verticalAxisDataLayer is null)
            {
                throw new ArgumentNullException(nameof(verticalAxisDataLayer));
            }

            if (setupStatusProvider is null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            this.verticalAxis = verticalAxisDataLayer;
            this.setupStatusProvider = setupStatusProvider;
            this.configurationProvider = dataLayerConfigurationValueManagement;
            this.resolutionCalibration = resolutionCalibration;
        }

        #endregion

        #region Methods

        [HttpPost("complete")]
        public IActionResult Complete(decimal newResolution)
        {
            this.ExecuteStep(newResolution, ResolutionCalibrationStep.CloseProcedure);

            this.verticalAxis.Resolution = newResolution; // TODO move this into state machine

            this.setupStatusProvider.CompleteVerticalResolution(); // TODO move this into state machine

            return this.Ok();
        }

        [HttpGet("adjusted-resolution")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult<decimal> GetAdjustedResolution(decimal measuredDistance, decimal expectedDistance)
        {
            if (measuredDistance <= 0)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Detail = "Measured distance must be strictly positive."
                    });
            }

            if (expectedDistance <= 0)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Detail = "Expected distance must be strictly positive."
                    });
            }

            var resolution = this.configurationProvider.GetDecimalConfigurationValue(
                    (long)VerticalAxis.Resolution,
                    ConfigurationCategory.VerticalAxis);

            return resolution * measuredDistance / expectedDistance;
        }

        [HttpGet("parameters")]
        public ActionResult<ResolutionCalibrationParameters> GetParameters()
        {
            var parameters = new ResolutionCalibrationParameters
            {
                CurrentResolution = this.configurationProvider.GetDecimalConfigurationValue(
                    (long)VerticalAxis.Resolution,
                    ConfigurationCategory.VerticalAxis),

                InitialPosition = this.configurationProvider.GetDecimalConfigurationValue(
                    (long)ResolutionCalibration.InitialPosition,
                    ConfigurationCategory.ResolutionCalibration),

                FinalPosition = this.configurationProvider.GetDecimalConfigurationValue(
                    (long)ResolutionCalibration.FinalPosition,
                    ConfigurationCategory.ResolutionCalibration),
            };

            return this.Ok(parameters);
        }

        [HttpPost("move-to-initial-position")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToInitialPosition(decimal position)
        {
            return this.ExecuteStep(position, ResolutionCalibrationStep.InitialPosition);
        }

        [HttpPost("move-to-position")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult MoveToPosition(decimal position)
        {
            return this.ExecuteStep(position, ResolutionCalibrationStep.Move);
        }

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesDefaultResponseType]
        public IActionResult Start(decimal position)
        {
            return this.ExecuteStep(position, ResolutionCalibrationStep.StartProcedure);
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

        private IActionResult ExecuteStep(decimal position, ResolutionCalibrationStep resolutionCalibrationStep)
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

        #endregion
    }
}
