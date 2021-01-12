using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/loading-units")]
    [ApiController]
    public class LoadingUnitsMasWmsController : ControllerBase
    {
        #region Constructors

        public LoadingUnitsMasWmsController()
        {
        }

        #endregion

        #region Methods

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
