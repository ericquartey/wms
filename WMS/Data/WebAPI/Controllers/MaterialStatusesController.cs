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
    public class MaterialStatusesController :
        ControllerBase,
        IReadAllController<MaterialStatus>,
        IReadSingleController<MaterialStatus, int>
    {
        #region Fields

        private readonly IMaterialStatusProvider materialStatusProvider;

        private readonly ILogger logger;

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

        [ProducesResponseType(200, Type = typeof(IEnumerable<MaterialStatus>))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaterialStatus>>> GetAllAsync()
        {
            return this.Ok(await this.materialStatusProvider.GetAllAsync());
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [HttpGet]
        [Route("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.materialStatusProvider.GetAllCountAsync());
        }

        [ProducesResponseType(200, Type = typeof(MaterialStatus))]
        [ProducesResponseType(404)]
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
