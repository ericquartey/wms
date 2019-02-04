using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionsController : ControllerBase,
        IReadAllController<Mission>,
        IReadSingleController<Mission>
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IMissionProvider missionProvider;

        #endregion

        #region Constructors

        public MissionsController(
            ILogger<ItemsController> logger,
            IMissionProvider missionProvider)
        {
            this.logger = logger;
            this.missionProvider = missionProvider;
        }

        #endregion

        #region Methods

        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Mission>))]
        public async Task<ActionResult<IEnumerable<Mission>>> GetAllAsync()
        {
            return this.Ok(await this.missionProvider.GetAllAsync());
        }

        [ProducesResponseType(200, Type = typeof(Mission))]
        [ProducesResponseType(404, Type = typeof(SerializableError))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("{id}")]
        public async Task<ActionResult<Mission>> GetByIdAsync(int id)
        {
            try
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
            catch (Exception ex)
            {
                var message = $"An error occurred while retrieving the requested entity with id={id}.";
                this.logger.LogError(ex, message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        #endregion
    }
}
