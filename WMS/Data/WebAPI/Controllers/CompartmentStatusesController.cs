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

        [ProducesResponseType(typeof(IEnumerable<CompartmentStatus>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompartmentStatus>>> GetAllAsync()
        {
            return this.Ok(await this.compartmentStatusProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.compartmentStatusProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(CompartmentStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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
