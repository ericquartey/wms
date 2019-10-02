using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataLayer.Providers.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.FiniteStateMachines.Providers;
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

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly IOffsetCalibrationDataLayer offsetCalibration;

        private readonly ISetupStatusProvider setupStatusProvider;

        #endregion

        #region Constructors

        public VerticalOffsetProcedureController(
            IEventAggregator eventAggregator,
            IElevatorProvider elevatorProvider,
            IElevatorDataProvider elevatorDataProvider,
            IConfigurationValueManagmentDataLayer configurationProvider,
            IOffsetCalibrationDataLayer offsetCalibrationDataLayer,
            ISetupStatusProvider setupStatusProvider)
            : base(eventAggregator)
        {
            if (elevatorDataProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorDataProvider));
            }

            if (configurationProvider is null)
            {
                throw new ArgumentNullException(nameof(configurationProvider));
            }

            if (offsetCalibrationDataLayer is null)
            {
                throw new ArgumentNullException(nameof(offsetCalibrationDataLayer));
            }

            if (setupStatusProvider is null)
            {
                throw new ArgumentNullException(nameof(setupStatusProvider));
            }

            this.elevatorProvider = elevatorProvider;
            this.elevatorDataProvider = elevatorDataProvider;
            this.configurationProvider = configurationProvider;
            this.offsetCalibration = offsetCalibrationDataLayer;
            this.setupStatusProvider = setupStatusProvider;
        }

        #endregion

        #region Methods

        [HttpPost("complete")]
        public IActionResult Complete(double newOffset)
        {
            this.elevatorDataProvider.UpdateVerticalOffset(newOffset);

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
                    OffsetCalibration.ReferenceCell,
                    category),

                StepValue = (double)this.configurationProvider.GetDecimalConfigurationValue(
                    OffsetCalibration.StepValue,
                    category),

                VerticalOffset = this.elevatorDataProvider.GetVerticalAxis().Offset
            };

            return this.Ok(parameters);
        }

        [HttpPost("move-down")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveDown()
        {
            var stepValue = (double)this.offsetCalibration.StepValue;

            this.elevatorProvider.MoveVerticalOfDistance(-stepValue, this.BayNumber, (double)this.offsetCalibration.FeedRateOC);

            return this.Accepted();
        }

        [HttpPost("move-up")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveUp()
        {
            var stepValue = (double)this.offsetCalibration.StepValue;

            this.elevatorProvider.MoveVerticalOfDistance(stepValue, this.BayNumber, (double)this.offsetCalibration.FeedRateOC);

            return this.Accepted();
        }

        #endregion
    }
}
