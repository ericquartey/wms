using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulerRequestsController :
        ControllerBase,
        IReadAllPagedController<SchedulerRequest>,
        IReadSingleController<SchedulerRequest, int>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly ISchedulerRequestProvider schedulerRequestProvider;

        #endregion

        #region Constructors

        public SchedulerRequestsController(
            ISchedulerRequestProvider schedulerRequestProvider)
        {
            this.schedulerRequestProvider = schedulerRequestProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(200, Type = typeof(IEnumerable<SchedulerRequest>))]
        [ProducesResponseType(400)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SchedulerRequest>>> GetAllAsync(
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
                    await this.schedulerRequestProvider.GetAllAsync(
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

                return await this.schedulerRequestProvider.GetAllCountAsync(
                           whereExpression,
                           search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(SchedulerRequest))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<SchedulerRequest>> GetByIdAsync(int id)
        {
            var result = await this.schedulerRequestProvider.GetByIdAsync(id);
            if (result == null)
            {
                return this.NotFound();
            }

            return this.Ok(result);
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<object>))]
        [ProducesResponseType(400)]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(string propertyName)
        {
            try
            {
                return this.Ok(await this.schedulerRequestProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        #endregion
    }
}
