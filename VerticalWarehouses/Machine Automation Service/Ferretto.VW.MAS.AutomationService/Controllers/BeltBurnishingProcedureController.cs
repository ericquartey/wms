using System;
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
    public class BeltBurnishingProcedureController : BaseAutomationController
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer configurationProvider;

        private readonly ISetupStatusProvider setupStatusProvider;

        private readonly IVerticalAxisDataLayer verticalAxis;

        #endregion

        #region Constructors

        public BeltBurnishingProcedureController(
            IEventAggregator eventAggregator,
            IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement,
            ISetupStatusProvider setupStatusProvider,
            IVerticalAxisDataLayer verticalAxisDataLayer)
            : base(eventAggregator)
        {
            if (dataLayerConfigurationValueManagement is null)
            {
                throw new ArgumentNullException(nameof(dataLayerConfigurationValueManagement));
            }

            if (setupStatusProvider is null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            if (verticalAxisDataLayer is null)
            {
                throw new ArgumentNullException(nameof(verticalAxisDataLayer));
            }

            this.configurationProvider = dataLayerConfigurationValueManagement;
            this.setupStatusProvider = setupStatusProvider;
            this.verticalAxis = verticalAxisDataLayer;
        }

        #endregion

        #region Methods

        [HttpGet("parameters")]
        public ActionResult<BeltBurnishingParameters> GetParameters()
        {
            var parameters = new BeltBurnishingParameters
            {
                UpperBound = this.configurationProvider.GetDecimalConfigurationValue(
                        (long)VerticalAxis.UpperBound,
                        ConfigurationCategory.VerticalAxis),
                LowerBound = this.configurationProvider.GetDecimalConfigurationValue(
                        (long)VerticalAxis.LowerBound,
                        ConfigurationCategory.VerticalAxis),
                RequiredCycles = this.configurationProvider.GetIntegerConfigurationValue(
                        (long)BeltBurnishing.CycleQuantity,
                         ConfigurationCategory.BeltBurnishing),
            };

            return this.Ok(parameters);
        }

        [HttpPost("mark-as-completed")]
        public IActionResult MarkAsCompleted()
        {
            this.setupStatusProvider.CompleteBeltBurnishing();

            return this.Ok();
        }

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult Start(decimal upperBoundPosition, decimal lowerBoundPosition, int totalTestCycleCount, int delayStart)
        {
            var parameters = new BeltBurnishingParameters
            {
                UpperBound = this.configurationProvider.GetDecimalConfigurationValue(
                        (long)VerticalAxis.UpperBound,
                        ConfigurationCategory.VerticalAxis),
                LowerBound = this.configurationProvider.GetDecimalConfigurationValue(
                        (long)VerticalAxis.LowerBound,
                        ConfigurationCategory.VerticalAxis)
            };

            if (upperBoundPosition <= 0)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Title = Resources.General.BadRequestTitle,
                        Detail = Resources.BeltBurnishingProcedure.UpperBoundPositionMustBeStrictlyPositive
                    });
            }

            if (upperBoundPosition > parameters.UpperBound)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Title = Resources.General.BadRequestTitle,
                        Detail = Resources.BeltBurnishingProcedure.UpperBoundPositionOutOfRange
                    });
            }

            if (upperBoundPosition <= lowerBoundPosition)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Title = Resources.General.BadRequestTitle,
                        Detail = Resources.BeltBurnishingProcedure.UpperBoundPositionMustBeStrictlyGreaterThanLowerBoundPosition
                    });
            }

            if (lowerBoundPosition < parameters.LowerBound)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Title = Resources.General.BadRequestTitle,
                        Detail = Resources.BeltBurnishingProcedure.LowerBoundPositionOutOfRange
                    });
            }

            if (totalTestCycleCount <= 0)
            {
                return this.BadRequest(
                    new ProblemDetails
                    {
                        Title = Resources.General.BadRequestTitle,
                        Detail = Resources.BeltBurnishingProcedure.TheNumberOfTestCyclesMustBeStrictlyPositive
                    });
            }

            var data = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                MovementMode.BeltBurnishing,
                upperBoundPosition,
                this.verticalAxis.MaxEmptySpeed,
                this.verticalAxis.MaxEmptyAcceleration,
                this.verticalAxis.MaxEmptyDeceleration,
                totalTestCycleCount,
                lowerBoundPosition,
                upperBoundPosition,
                delayStart);

            this.PublishCommand(
                data,
                "Execute Belt Burnishing Command",
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

        #endregion
    }
}
