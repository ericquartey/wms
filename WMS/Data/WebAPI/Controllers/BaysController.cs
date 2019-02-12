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

        [ProducesResponseType(200, Type = typeof(Bay))]
        [HttpPost("{id}/activate")]
        public async Task<ActionResult<IEnumerable<Bay>>> ActivateAsync(int id)
        {
            return this.Ok(await this.bayProvider.ActivateAsync(id));
        }

        [ProducesResponseType(200, Type = typeof(Bay))]
        [HttpPost("{id}/deactivate")]
        public async Task<ActionResult<IEnumerable<Bay>>> DeactivateAsync(int id)
        {
            return this.Ok(await this.bayProvider.DeactivateAsync(id));
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<Bay>))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bay>>> GetAllAsync()
        {
            return this.Ok(await this.bayProvider.GetAllAsync());
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [HttpGet]
        [Route("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.bayProvider.GetAllCountAsync());
        }

        [ProducesResponseType(200, Type = typeof(Bay))]
        [ProducesResponseType(404)]
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
