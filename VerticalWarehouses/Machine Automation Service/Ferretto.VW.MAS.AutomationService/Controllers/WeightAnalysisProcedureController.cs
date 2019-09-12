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
        public IActionResult Start(decimal displacement, decimal netWeight, int? loadingUnitId)
        {
            try
            {
                this.elevatorProvider.RunTorqueCurrentSampling(displacement, netWeight, loadingUnitId);

                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        [HttpPost("stop")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesDefaultResponseType]
        public IActionResult Stop()
        {
            try
            {
                this.elevatorProvider.Stop();

                return this.Accepted();
            }
            catch (Exception ex)
            {
                return this.NegativeResponse(ex);
            }
        }

        #endregion
    }
}
