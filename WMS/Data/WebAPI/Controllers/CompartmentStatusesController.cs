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
    public class CompartmentStatusesController :
        ControllerBase,
        IReadAllController<CompartmentStatus>,
        IReadSingleController<CompartmentStatus, int>
    {
        #region Fields

        private readonly ICompartmentStatusProvider compartmentStatusProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public CompartmentStatusesController(
            ILogger<CompartmentStatusesController> logger,
            ICompartmentStatusProvider compartmentStatusProvider)
        {
            this.logger = logger;
            this.compartmentStatusProvider = compartmentStatusProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<CompartmentStatus>))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompartmentStatus>>> GetAllAsync()
        {
            return this.Ok(await this.compartmentStatusProvider.GetAllAsync());
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.compartmentStatusProvider.GetAllCountAsync());
        }

        [ProducesResponseType(200, Type = typeof(CompartmentStatus))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CompartmentStatus>> GetByIdAsync(int id)
        {
            var result = await this.compartmentStatusProvider.GetByIdAsync(id);
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
