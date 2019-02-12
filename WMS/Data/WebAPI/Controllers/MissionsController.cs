using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionsController :
        ControllerBase,
        IReadAllController<Mission>,
        IReadSingleController<Mission, int>
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IMissionProvider missionProvider;

        #endregion

        #region Constructors

        public MissionsController(
            ILogger<MissionsController> logger,
            IMissionProvider missionProvider)
        {
            this.logger = logger;
            this.missionProvider = missionProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(Mission))]
        [ProducesResponseType(404, Type = typeof(string))]
        [HttpPost("{id}/abort")]
        public Task<ActionResult<Mission>> Abort(int id)
        {
            throw new System.NotImplementedException();
        }

        [ProducesResponseType(200, Type = typeof(Mission))]
        [ProducesResponseType(404, Type = typeof(string))]
        [HttpPost("{id}/complete")]
        public Task<ActionResult<Mission>> Complete(int id)
        {
            throw new System.NotImplementedException();
        }

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Mission>))]
        public async Task<ActionResult<IEnumerable<Mission>>> GetAllAsync()
        {
            return this.Ok(await this.missionProvider.GetAllAsync());
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [HttpGet]
        [Route("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.missionProvider.GetAllCountAsync());
        }

        [ProducesResponseType(200, Type = typeof(Mission))]
        [ProducesResponseType(404, Type = typeof(string))]
        [HttpGet("{id}")]
        public async Task<ActionResult<Mission>> GetByIdAsync(int id)
        {
            var result = await this.missionProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(message);
            }

            return this.Ok(result);
        }

        #endregion
    }
}
