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
    public class LoadingUnitTypesController :
        ControllerBase,
        IReadAllController<LoadingUnitType>,
        IReadSingleController<LoadingUnitType, int>
    {
        #region Fields

        private readonly ILoadingUnitTypeProvider loadingUnitTypeProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public LoadingUnitTypesController(
            ILogger<LoadingUnitTypesController> logger,
            ILoadingUnitTypeProvider loadingUnitTypeProvider)
        {
            this.logger = logger;
            this.loadingUnitTypeProvider = loadingUnitTypeProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<LoadingUnitType>))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoadingUnitType>>> GetAllAsync()
        {
            return this.Ok(await this.loadingUnitTypeProvider.GetAllAsync());
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.loadingUnitTypeProvider.GetAllCountAsync());
        }

        [ProducesResponseType(200, Type = typeof(LoadingUnitType))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<LoadingUnitType>> GetByIdAsync(int id)
        {
            var result = await this.loadingUnitTypeProvider.GetByIdAsync(id);
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
