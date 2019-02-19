using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MissionsController :
        ControllerBase,
        IReadAllPagedController<Mission>,
        IReadSingleController<Mission, int>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly ILogger logger;

        private readonly IMissionProvider missionProvider;

        #endregion

        #region Constructors

        public MissionsController(
            ILogger<MissionsController> logger,
            IMissionProvider missionProvider)
        {
            this.logger = logger;
            this.missionProvider = missionProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(Mission))]
        [ProducesResponseType(404, Type = typeof(string))]
        [HttpPost("{id}/abort")]
        public Task<ActionResult<Mission>> Abort(int id)
        {
            throw new System.NotImplementedException();
        }

        [ProducesResponseType(200, Type = typeof(Mission))]
        [ProducesResponseType(404, Type = typeof(string))]
        [HttpPost("{id}/complete")]
        public Task<ActionResult<Mission>> Complete(int id)
        {
            throw new System.NotImplementedException();
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<Mission>))]
        [ProducesResponseType(400, Type = typeof(string))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Mission>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            try
            {
                var whereExpression = where.AsIExpression();
                var orderByExpression = orderBy.ParseSortOptions();

                return this.Ok(
                    await this.missionProvider.GetAllAsync(
                        skip,
                        take,
                        orderByExpression,
                        whereExpression,
                        search));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(400, Type = typeof(string))]
        [ProducesResponseType(404)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(
            string where = null,
            string search = null)
        {
            try
            {
                var whereExpression = where.AsIExpression();

                return await this.missionProvider.GetAllCountAsync(
                           whereExpression,
                           search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(Mission))]
        [ProducesResponseType(404, Type = typeof(string))]
        [HttpGet("{id}")]
        public async Task<ActionResult<Mission>> GetByIdAsync(int id)
        {
            var result = await this.missionProvider.GetByIdAsync(id);
            if (result == null)
            {
                var message = $"No entity with the specified id={id} exists.";
                this.logger.LogWarning(message);
                return this.NotFound(message);
            }

            return this.Ok(result);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [ProducesResponseType(400)]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(
            string propertyName)
        {
            try
            {
                return this.Ok(await this.missionProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        #endregion
    }
}
