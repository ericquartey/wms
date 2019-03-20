using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompartmentTypesController :
        ControllerBase,
        IReadAllController<CompartmentType>,
        IReadSingleController<CompartmentType, int>
    {
        #region Fields

        private readonly ICompartmentTypeProvider compartmentTypeProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public CompartmentTypesController(
            ILogger<CompartmentTypesController> logger,
            ICompartmentTypeProvider compartmentTypeProvider)
        {
            this.logger = logger;
            this.compartmentTypeProvider = compartmentTypeProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(CompartmentType), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<CompartmentType>> CreateAsync(
            CompartmentType model,
            int? itemId,
            int? maxCapacity)
        {
            var result = await this.compartmentTypeProvider.CreateAsync(model, itemId, maxCapacity);

            if (!result.Success)
            {
                return this.BadRequest();
            }

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [ProducesResponseType(typeof(IEnumerable<CompartmentType>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompartmentType>>> GetAllAsync()
        {
            return this.Ok(await this.compartmentTypeProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.compartmentTypeProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(CompartmentType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<CompartmentType>> GetByIdAsync(int id)
        {
            var result = await this.compartmentTypeProvider.GetByIdAsync(id);
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
