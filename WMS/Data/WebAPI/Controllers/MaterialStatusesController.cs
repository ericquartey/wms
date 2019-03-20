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
    public class MaterialStatusesController :
        ControllerBase,
        IReadAllController<MaterialStatus>,
        IReadSingleController<MaterialStatus, int>
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IMaterialStatusProvider materialStatusProvider;

        #endregion

        #region Constructors

        public MaterialStatusesController(
            ILogger<MaterialStatusesController> logger,
            IMaterialStatusProvider materialStatusProvider)
        {
            this.logger = logger;
            this.materialStatusProvider = materialStatusProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(IEnumerable<MaterialStatus>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaterialStatus>>> GetAllAsync()
        {
            return this.Ok(await this.materialStatusProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.materialStatusProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(MaterialStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<MaterialStatus>> GetByIdAsync(int id)
        {
            var result = await this.materialStatusProvider.GetByIdAsync(id);
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
