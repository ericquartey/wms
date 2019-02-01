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
    public class BaysController : ControllerBase,
        IReadAllController<Bay>,
        IReadSingleController<Bay>
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly ILogger logger;

        #endregion Fields

        #region Constructors

        public BaysController(
            ILogger<BaysController> logger,
            IBayProvider bayProvider)
        {
            this.logger = logger;
            this.bayProvider = bayProvider;
        }

        #endregion Constructors

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<Bay>))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bay>>> GetAll()
        {
            return this.Ok(await this.bayProvider.GetAllAsync());
        }

        [ProducesResponseType(200, Type = typeof(Bay))]
        [ProducesResponseType(404)]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("{id}")]
        public async Task<ActionResult<Bay>> GetById(int id)
        {
            try
            {
                var result = await this.bayProvider.GetByIdAsync(id);
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

        #endregion Methods
    }
}
