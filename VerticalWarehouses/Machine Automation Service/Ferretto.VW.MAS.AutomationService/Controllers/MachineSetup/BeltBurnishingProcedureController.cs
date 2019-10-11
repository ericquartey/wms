using System;
using Ferretto.VW.CommonUtils.Messages.Data;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.AutomationService.Models;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.VW.MAS.DataLayer.Interfaces;
using Ferretto.VW.MAS.DataModels.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Providers.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BeltBurnishingProcedureController : BaseAutomationController
    {
        #region Fields

        private readonly IElevatorDataProvider elevatorDataProvider;

        private readonly IElevatorProvider elevatorProvider;

        private readonly ISetupProceduresDataProvider setupProceduresDataProvider;

        private readonly ISetupStatusProvider setupStatusProvider;

        #endregion

        #region Constructors

        public BeltBurnishingProcedureController(
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

        [HttpGet("parameters")]
        public ActionResult<BeltBurnishingParameters> GetParameters()
        {
            var verticalAxis = this.elevatorDataProvider.GetVerticalAxis();

            var setupProcedures = this.setupProceduresDataProvider.GetAll();

            var parameters = new BeltBurnishingParameters
            {
                UpperBound = verticalAxis.UpperBound,
                LowerBound = verticalAxis.LowerBound,
                RequiredCycles = setupProcedures.BeltBurnishingTest.RequiredCycles,
            };

            return this.Ok(parameters);
        }

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult Start(double upperBoundPosition, double lowerBoundPosition, int totalTestCycleCount, int delayStart)
        {
            this.elevatorProvider.RepeatVerticalMovement(upperBoundPosition, lowerBoundPosition, totalTestCycleCount, delayStart, this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.elevatorProvider.Stop(this.BayNumber, MessageActor.WebApi);

            return this.Accepted();
        }

        #endregion
    }
}
