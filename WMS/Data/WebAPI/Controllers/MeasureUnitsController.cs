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
    public class MeasureUnitsController :
        ControllerBase,
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

        [ProducesResponseType(200, Type = typeof(IEnumerable<MeasureUnit>))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MeasureUnit>>> GetAllAsync()
        {
            return this.Ok(await this.measureUnitProvider.GetAllAsync());
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [HttpGet]
        [Route("count")]
        public async Task<ActionResult<int>> GetAllCountAsync()
        {
            return this.Ok(await this.measureUnitProvider.GetAllCountAsync());
        }

        [ProducesResponseType(200, Type = typeof(MeasureUnit))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<MeasureUnit>> GetByIdAsync(string id)
        {
            var result = await this.measureUnitProvider.GetByIdAsync(id);
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
