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
    public class BaysController :
        ControllerBase,
        IReadAllController<Bay>,
        IReadSingleController<Bay, int>
    {
        #region Fields

        private readonly IBayProvider bayProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public BaysController(
            ILogger<BaysController> logger,
            IBayProvider bayProvider)
        {
            this.logger = logger;
            this.bayProvider = bayProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(Bay), StatusCodes.Status200OK)]
        [HttpPost("{id}/activate")]
        public async Task<ActionResult<IEnumerable<Bay>>> ActivateAsync(int id)
        {
            return this.Ok(await this.bayProvider.ActivateAsync(id));
        }

        [ProducesResponseType(typeof(Bay), StatusCodes.Status200OK)]
        [HttpPost("{id}/deactivate")]
        public async Task<ActionResult<IEnumerable<Bay>>> DeactivateAsync(int id)
        {
            return this.Ok(await this.bayProvider.DeactivateAsync(id));
        }

        [ProducesResponseType(typeof(IEnumerable<Bay>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bay>>> GetAllAsync()
        {
            return this.Ok(await this.bayProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.bayProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(Bay), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<Bay>> GetByIdAsync(int id)
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

        #endregion
    }
}
