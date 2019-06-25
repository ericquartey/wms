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
    public class MeasureUnitsController :
        BaseController,
        IReadAllController<MeasureUnit>,
        IReadSingleController<MeasureUnit, string>
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IMeasureUnitProvider measureUnitProvider;

        #endregion

        #region Constructors

        public MeasureUnitsController(
            ILogger<MeasureUnitsController> logger,
            IMeasureUnitProvider measureUnitProvider)
        {
            this.logger = logger;
            this.measureUnitProvider = measureUnitProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(IEnumerable<MeasureUnit>), StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MeasureUnit>>> GetAllAsync()
        {
            return this.Ok(await this.measureUnitProvider.GetAllAsync());
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.measureUnitProvider.GetAllCountAsync());
        }

        [ProducesResponseType(typeof(MeasureUnit), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<MeasureUnit>> GetByIdAsync(string id)
        {
            var result = await this.measureUnitProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = string.Format(WMS.Data.Resources.Errors.NoEntityExists, id);
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
