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
    public class AbcClassesController :
        ControllerBase,
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

        [ProducesResponseType(typeof(IEnumerable<AbcClass>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AbcClass>>> GetAllAsync()
        {
            return this.Ok(await this.abcClassProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.abcClassProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(AbcClass), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<AbcClass>> GetByIdAsync(string id)
        {
            var result = await this.abcClassProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(new ProblemDetails
                {
                    Detail = message,
                    Status = StatusCodes.Status404NotFound
                });
            }

            return this.Ok(result);
        }

        #endregion
    }
}
