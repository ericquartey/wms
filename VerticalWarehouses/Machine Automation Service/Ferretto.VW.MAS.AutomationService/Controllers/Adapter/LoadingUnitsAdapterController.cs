using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Contracts;
using Ferretto.VW.MAS.AutomationService.Models;
//using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/loading-units")]
    [ApiController]
    public class LoadingUnitsAdapterController : ControllerBase
    {
        #region Constructors

        public LoadingUnitsAdapterController()
        {
        }

        #endregion

        #region Methods

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [HttpPost("{id}/save")]
        public async Task<ActionResult> SaveAsync(int id, [FromBody] LoadingUnitDetails info)
        {
            throw new NotSupportedException();
        }

        [ProducesResponseType(typeof(LoadingUnitDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<ActionResult<LoadingUnitDetails>> UpdateAsync(LoadingUnitDetails model, int id)
        {
            throw new NotSupportedException();
        }

        [HttpPost("{id}/withdraw")]
        [ProducesResponseType(typeof(LoadingUnitSchedulerRequest), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<LoadingUnitSchedulerRequest>> WithdrawAsync(int id, int bayId)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}
