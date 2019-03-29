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

        [ProducesResponseType(typeof(IEnumerable<LoadingUnitType>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoadingUnitType>>> GetAllAsync()
        {
            return this.Ok(await this.loadingUnitTypeProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.loadingUnitTypeProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(LoadingUnitType), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<LoadingUnitType>> GetByIdAsync(int id)
        {
            var result = await this.loadingUnitTypeProvider.GetByIdAsync(id);
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
