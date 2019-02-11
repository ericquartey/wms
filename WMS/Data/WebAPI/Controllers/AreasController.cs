using System;
using System.Collections.Generic;
using System.Linq;
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
    public class AreasController :
        ControllerBase,
        IReadAllController<Area>,
        IReadSingleController<Area, int>
    {
        #region Fields

        private readonly IAreaProvider areaProvider;

        private readonly IBayProvider bayProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public AreasController(
            ILogger<AreasController> logger,
            IAreaProvider areaProvider,
            IBayProvider bayProvider)
        {
            this.logger = logger;
            this.areaProvider = areaProvider;
            this.bayProvider = bayProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<Area>))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Area>>> GetAllAsync()
        {
            try
            {
                return this.Ok(await this.areaProvider.GetAllAsync());
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while retrieving the requested entities.";
                this.logger.LogError(ex, message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [HttpGet]
        [Route("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.areaProvider.GetAllCountAsync());
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<Bay>))]
        [ProducesResponseType(404)]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("{id}/bays")]
        public async Task<ActionResult<IEnumerable<Bay>>> GetBaysAsync(int id)
        {
            try
            {
                var bays = await this.bayProvider.GetByAreaIdAsync(id);

                if (!bays.Any())
                {
                    var message = $"No entity associated with the specified id={id} exists.";
                    this.logger.LogWarning(message);
                    return this.NotFound(message);
                }

                return this.Ok(bays);
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while retrieving the requested entities associated with id={id}.";
                this.logger.LogError(ex, message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [ProducesResponseType(200, Type = typeof(Area))]
        [ProducesResponseType(404)]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("{id}")]
        public async Task<ActionResult<Area>> GetByIdAsync(int id)
        {
            try
            {
                var result = await this.areaProvider.GetByIdAsync(id);
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
