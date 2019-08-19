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
    public class BeltBurnishingProcedureController : BaseAutomationController
    {
        #region Fields

        private readonly IConfigurationValueManagmentDataLayer dataLayerConfigurationValueManagement;

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
            if (dataLayerConfigurationValueManagement == null)
            {
                throw new ArgumentNullException(nameof(dataLayerConfigurationValueManagement));
            }

            if (setupStatusProvider == null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            if (verticalAxisDataLayer == null)
            {
                throw new ArgumentNullException(nameof(verticalAxisDataLayer));
            }

            this.dataLayerConfigurationValueManagement = dataLayerConfigurationValueManagement;
            this.setupStatusProvider = setupStatusProvider;
            this.verticalAxis = verticalAxisDataLayer;
        }

        #endregion

        #region Methods

        [HttpGet("decimal-configuration-parameter/{category}/{parameter}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<decimal> GetDecimalConfigurationParameter(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            var categoryEnum = (ConfigurationCategory)categoryId;

            var parseSuccess = Enum.TryParse(typeof(VerticalAxis), parameter, out var verticalAxisParameterId);

            if (parseSuccess)
            {
                decimal value1;

                try
                {
                    value1 = this.dataLayerConfigurationValueManagement
                        .GetDecimalConfigurationValue(
                        (long)verticalAxisParameterId,
                        categoryEnum);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
                {
                    return this.NotFound(new ProblemDetails { Title = "Parameter not found" });
                }

                return this.Ok(value1);
            }
            else
            {
                return this.NotFound(new ProblemDetails { Title = "Parameter not found" });
            }
        }

        [HttpGet("integer-configuration-parameter/{category}/{parameter}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<int> GetIntegerConfigurationParameter(string category, string parameter)
        {
            Enum.TryParse(typeof(ConfigurationCategory), category, out var categoryId);
            Enum.TryParse(typeof(BeltBurnishing), parameter, out var parameterId);
            var categoryEnum = (ConfigurationCategory)categoryId;

            if (parameterId != null)
            {
                int value;

                try
                {
                    value = this.dataLayerConfigurationValueManagement
                        .GetIntegerConfigurationValue(
                        (long)parameterId,
                        categoryEnum);
                }
                catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
                {
                    return this.NotFound(new ProblemDetails { Title = "Parameter not found" });
                }

                return this.Ok(value);
            }
            else
            {
                return this.NotFound(new ProblemDetails { Title = "Parameter not found" });
            }
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
        public IActionResult Start(decimal upperBound, decimal lowerBound, int requiredCycles)
        {
            if (upperBound <= 0)
            {
                return this.BadRequest(new ProblemDetails { Detail = "Upper bound cannot be negative or zero." });
            }

            if (upperBound <= lowerBound)
            {
                return this.BadRequest(new ProblemDetails { Detail = "Upper bound must be strictly greater than lower bound." });
            }

            if (requiredCycles <= 0)
            {
                return this.BadRequest(new ProblemDetails { Detail = "Required cycles count must be strictly positive." });
            }

            var data = new PositioningMessageData(
                Axis.Vertical,
                MovementType.Absolute,
                MovementMode.BeltBurnishing,
                upperBound,
                this.verticalAxis.MaxEmptySpeed,
                this.verticalAxis.MaxEmptyAcceleration,
                this.verticalAxis.MaxEmptyDeceleration,
                requiredCycles,
                lowerBound,
                upperBound);

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
