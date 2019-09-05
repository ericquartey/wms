using System;
using System.IO;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
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

        private readonly IConfigurationValueManagmentDataLayer configurationProvider;

        private readonly IOffsetCalibrationDataLayer offsetCalibration;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalAxisDataLayer verticalAxis;

        #endregion

        #region Constructors

        public VerticalOffsetProcedureController(
            IEventAggregator eventAggregator,
            IConfigurationValueManagmentDataLayer configurationProvider,
            IVerticalAxisDataLayer verticalAxisDataLayer,
            IOffsetCalibrationDataLayer offsetCalibrationDataLayer,
            ISetupStatusProvider setupStatusProvider)
            : base(eventAggregator)
        {
            if (configurationProvider is null)
            {
                throw new ArgumentNullException(nameof(configurationProvider));
            }

            if (verticalAxisDataLayer is null)
            {
                throw new ArgumentNullException(nameof(verticalAxisDataLayer));
            }

            if (offsetCalibrationDataLayer is null)
            {
                throw new ArgumentNullException(nameof(offsetCalibrationDataLayer));
            }

            if (setupStatusProvider is null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            this.configurationProvider = configurationProvider;
            this.verticalAxis = verticalAxisDataLayer;
            this.offsetCalibration = offsetCalibrationDataLayer;
            this.setupStatusProvider = setupStatusProvider;
        }

        #endregion

        #region Methods

        [HttpPost("complete")]
        public IActionResult Complete(decimal newOffset)
        {
            this.configurationProvider
                .SetDecimalConfigurationValue(
                    (long)VerticalAxis.Offset,
                    ConfigurationCategory.VerticalAxis,
                    newOffset);

            this.setupStatusProvider.CompleteVerticalOffset();

            return this.Ok();
        }

        [HttpGet("parameters")]
        public ActionResult<VerticalOffsetProcedureParameters> GetParameters()
        {
            var category = ConfigurationCategory.OffsetCalibration;

            var parameters = new VerticalOffsetProcedureParameters
            {
                ReferenceCellId = this.configurationProvider.GetIntegerConfigurationValue(
                    (long)OffsetCalibration.ReferenceCell,
                    category),

                StepValue = this.configurationProvider.GetDecimalConfigurationValue(
                    (long)OffsetCalibration.StepValue,
                    category),

                VerticalOffset = this.configurationProvider.GetDecimalConfigurationValue(
                    (long)VerticalAxis.Offset,
                    ConfigurationCategory.VerticalAxis)
            };

            return this.Ok(parameters);
        }

        [HttpPost("move-down")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveDown()
        {
            var stepValue = this.offsetCalibration.StepValue;

            this.ExecuteStep(-stepValue);

            return this.Accepted();
        }

        [HttpPost("move-up")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveUp()
        {
            var stepValue = this.offsetCalibration.StepValue;

            this.ExecuteStep(stepValue);

            return this.Accepted();
        }

        private void ExecuteStep(decimal displacement)
        {
            var maxSpeed = this.verticalAxis.MaxEmptySpeed;
            var feedRate = this.offsetCalibration.FeedRateOC;

            decimal[] speed = { maxSpeed * feedRate };
            decimal[] acceleration = { this.verticalAxis.MaxEmptyAcceleration };
            decimal[] deceleration = { this.verticalAxis.MaxEmptyDeceleration };
            decimal[] switchPosition = { 0 };

            var messageData = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Relative,
                MovementMode.Position,
                displacement,
                speed,
                acceleration,
                deceleration,
                0,
                0,
                0,
                0,
                switchPosition);

            this.PublishCommand(
                messageData,
                "Offset Calibration Start",
                MessageActor.FiniteStateMachines,
                MessageType.Positioning);
        }

        #endregion
    }
}
