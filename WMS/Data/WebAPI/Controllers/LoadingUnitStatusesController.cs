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
    public class LoadingUnitStatusesController :
        ControllerBase,
        IReadAllController<LoadingUnitStatus>,
        IReadSingleController<LoadingUnitStatus, string>
    {
        #region Fields

        private readonly ILoadingUnitStatusProvider loadingUnitStatusProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public LoadingUnitStatusesController(
            ILogger<LoadingUnitStatusesController> logger,
            ILoadingUnitStatusProvider loadingUnitStatusProvider)
        {
            this.logger = logger;
            this.loadingUnitStatusProvider = loadingUnitStatusProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(IEnumerable<LoadingUnitStatus>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoadingUnitStatus>>> GetAllAsync()
        {
            return this.Ok(await this.loadingUnitStatusProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.loadingUnitStatusProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(LoadingUnitStatus), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<LoadingUnitStatus>> GetByIdAsync(string id)
        {
            var result = await this.loadingUnitStatusProvider.GetByIdAsync(id);
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
