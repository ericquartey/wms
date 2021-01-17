using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/machines")]
    [ApiController]
    public class MachinesAdapterController : ControllerBase
    {
        #region Constructors

        public MachinesAdapterController()
        {
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(Area), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/area")]
        public Task<ActionResult<Area>> GetAreaByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        [ProducesResponseType(typeof(MachineDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<MachineDetails>> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        [ProducesResponseType(typeof(IEnumerable<LoadingUnitDetails>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/loading-units")]
        public async Task<ActionResult<LoadingUnitDetails>> GetLoadingUnitsByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        [ProducesResponseType(typeof(MissionInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/missions")]
        public async Task<ActionResult<MissionInfo>> GetMissionsByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
