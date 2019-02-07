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
    public class AbcClassesController : ControllerBase,
        IReadAllController<AbcClass>,
        IReadSingleController<AbcClass, string>
    {
        #region Fields

        private readonly IAbcClassProvider abcClassProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public AbcClassesController(
            ILogger<AbcClassesController> logger,
            IAbcClassProvider abcClassProvider)
        {
            this.logger = logger;
            this.abcClassProvider = abcClassProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<AbcClass>))]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AbcClass>>> GetAllAsync()
        {
            try
            {
                return this.Ok(await this.abcClassProvider.GetAllAsync());
            }
            catch (Exception ex)
            {
                var message = $"An error occurred while retrieving the requested entities.";
                this.logger.LogError(ex, message);
                return this.StatusCode(StatusCodes.Status500InternalServerError, message);
            }
        }

        [ProducesResponseType(200, Type = typeof(AbcClass))]
        [ProducesResponseType(404)]
        [ProducesResponseType(500, Type = typeof(string))]
        [HttpGet("{id}")]
        public async Task<ActionResult<AbcClass>> GetByIdAsync(string id)
        {
            try
            {
                var result = await this.abcClassProvider.GetByIdAsync(id);
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
