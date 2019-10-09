using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels;
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

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly ISetupStatusProvider setupStatusProvider;

        #endregion

        #region Constructors

        public VerticalOffsetProcedureController(
            IEventAggregator eventAggregator,
            IElevatorProvider elevatorProvider,
            IElevatorDataProvider elevatorDataProvider,
            ISetupProceduresDataProvider setupProceduresDataProvider,
            ISetupStatusProvider setupStatusProvider)
            : base(eventAggregator)
        {
            this.elevatorProvider = elevatorProvider ?? throw new ArgumentNullException(nameof(elevatorProvider));
            this.elevatorDataProvider = elevatorDataProvider ?? throw new ArgumentNullException(nameof(elevatorDataProvider));
            this.setupProceduresDataProvider = setupProceduresDataProvider ?? throw new ArgumentNullException(nameof(setupProceduresDataProvider));
            this.setupStatusProvider = setupStatusProvider ?? throw new ArgumentNullException(nameof(setupStatusProvider));
        }

        #endregion

        #region Methods

        [HttpPost("complete")]
        public IActionResult Complete(double newOffset)
        {
            this.elevatorDataProvider.UpdateVerticalOffset(newOffset);

            return this.Ok();
        }

        [HttpGet("parameters")]
        public ActionResult<OffsetCalibrationProcedure> GetParameters()
        {
            var procedureParameters = this.setupProceduresDataProvider.GetAll().OffsetCalibration;

            return this.Ok(procedureParameters);
        }

        [HttpPost("move-down")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveDown()
        {
            var procedureParameters = this.setupProceduresDataProvider.GetAll().OffsetCalibration;

            this.elevatorProvider.MoveVerticalOfDistance(-procedureParameters.Step, this.BayNumber, procedureParameters.FeedRate);

            return this.Accepted();
        }

        [HttpPost("move-up")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult MoveUp()
        {
            var procedureParameters = this.setupProceduresDataProvider.GetAll().OffsetCalibration;

            this.elevatorProvider.MoveVerticalOfDistance(procedureParameters.Step, this.BayNumber, procedureParameters.FeedRate);

            return this.Accepted();
        }

        #endregion
    }
}
