using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Extensions;
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
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = this.BuildWhereExpression<SchedulerRequest>(where);

                return this.Ok(
                    await this.schedulerRequestProvider.GetAllAsync(
                        skip,
                        take,
                        orderBy,
                        whereExpression,
                        searchExpression));
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
                var searchExpression = BuildSearchExpression(search);
                var whereExpression = this.BuildWhereExpression<SchedulerRequest>(where);

                return await this.schedulerRequestProvider.GetAllCountAsync(
                           whereExpression,
                           searchExpression);
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
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(string propertyName)
        {
            return this.Ok(await this.schedulerRequestProvider.GetUniqueValuesAsync(propertyName));
        }

        private static Expression<Func<SchedulerRequest, bool>> BuildSearchExpression(string search)
        {
            if (string.IsNullOrWhiteSpace(search))
            {
                return null;
            }

            return (i) =>
                i.AreaDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.BayDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ItemUnitMeasure.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ListDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.ListRowDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.LoadingUnitDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.LoadingUnitTypeDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.Lot.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.MaterialStatusDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.PackageTypeDescription.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.RegistrationNumber.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.Sub1.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.Sub2.Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.DispatchedQuantity.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase)
                ||
                i.DispatchedQuantity.ToString().Contains(search, StringComparison.InvariantCultureIgnoreCase);
        }

        #endregion
    }
}
