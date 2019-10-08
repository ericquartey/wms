using System;
using Ferretto.VW.CommonUtils.Messages.Enumerations;
using Ferretto.VW.MAS.DeviceManager.Providers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Prism.Events;

// ReSharper disable ArrangeThisQualifier
namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeightAnalysisProcedureController : BaseAutomationController
    {
        #region Fields

        private readonly IElevatorProvider elevatorProvider;

        #endregion

        #region Constructors

        public WeightAnalysisProcedureController(
            IEventAggregator eventAggregator,
            IElevatorProvider elevatorProvider)
            : base(eventAggregator)
        {
            if (elevatorProvider is null)
            {
                throw new ArgumentNullException(nameof(elevatorProvider));
            }

            this.elevatorProvider = elevatorProvider;
        }

        #endregion

        #region Methods

        [HttpPost("start")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public IActionResult Start(double displacement, double netWeight, int? loadingUnitId)
        {
            this.elevatorProvider.RunTorqueCurrentSampling(displacement, netWeight, loadingUnitId, this.BayNumber,
                MessageActor.AutomationService);

            return this.Accepted();
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            this.elevatorProvider.Stop(this.BayNumber, MessageActor.AutomationService);

            return this.Accepted();
        }

        #endregion
    }
}
