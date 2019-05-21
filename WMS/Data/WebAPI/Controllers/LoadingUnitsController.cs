using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Hubs;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.Hubs;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoadingUnitsController :
        BaseController,
        ICreateController<LoadingUnitCreating>,
        IReadAllPagedController<LoadingUnit>,
        IReadSingleController<LoadingUnitDetails, int>,
        IUpdateController<LoadingUnitDetails, int>,
        IDeleteController<int>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly ICompartmentProvider compartmentProvider;

        private readonly ILoadingUnitProvider loadingUnitProvider;

        private readonly ISchedulerService schedulerService;

        #endregion

        #region Constructors

        public LoadingUnitsController(
            IHubContext<DataHub, IDataHub> hubContext,
            ILoadingUnitProvider loadingUnitProvider,
            ICompartmentProvider compartmentProvider,
            ISchedulerService schedulerService)
            : base(hubContext)
        {
            this.loadingUnitProvider = loadingUnitProvider;
            this.compartmentProvider = compartmentProvider;
            this.schedulerService = schedulerService;
        }

        #endregion

        #region Methods

        [ProducesResponseType(typeof(LoadingUnitCreating), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<ActionResult<LoadingUnitCreating>> CreateAsync(LoadingUnitCreating model)
        {
            var result = await this.loadingUnitProvider.CreateAsync(model);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            await this.NotifyEntityUpdatedAsync(nameof(LoadingUnit), result.Entity.Id, HubEntityOperation.Created);

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(422)]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAsync(int id)
        {
            var result = await this.loadingUnitProvider.DeleteAsync(id);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            await this.NotifyEntityUpdatedAsync(nameof(LoadingUnit), id, HubEntityOperation.Deleted);

            return this.Ok();
        }

        [ProducesResponseType(typeof(IEnumerable<LoadingUnit>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LoadingUnit>>> GetAllAsync(
            int skip = 0,
            int take = 0,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            try
            {
                var orderByExpression = orderBy.ParseSortOptions();

                return this.Ok(
                    await this.loadingUnitProvider.GetAllAsync(
                        skip,
                        take,
                        orderByExpression,
                        where,
                        search));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(
            string where = null,
            string search = null)
        {
            try
            {
                return await this.loadingUnitProvider.GetAllCountAsync(where, search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(LoadingUnitDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}")]
        public async Task<ActionResult<LoadingUnitDetails>> GetByIdAsync(int id)
        {
            var result = await this.loadingUnitProvider.GetByIdAsync(id);
            if (result == null)
            {
                return this.NotFound(new ProblemDetails
                {
                    Detail = id.ToString(),
                    Status = StatusCodes.Status404NotFound,
                });
            }

            return this.Ok(result);
        }

        [ProducesResponseType(typeof(IEnumerable<CompartmentDetails>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/compartments")]
        public async Task<ActionResult<IEnumerable<CompartmentDetails>>> GetCompartmentsAsync(int id)
        {
            var compartments = await this.compartmentProvider.GetByLoadingUnitIdAsync(id);

            return this.Ok(compartments);
        }

        [ProducesResponseType(typeof(LoadingUnitSize), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpGet("{id}/size")]
        public async Task<ActionResult<LoadingUnitSize>> GetSizeByTypeIdAsync(int id)
        {
            var info = await this.loadingUnitProvider.GetSizeByTypeIdAsync(id);
            if (info == null)
            {
                return this.NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound
                });
            }

            return this.Ok(info);
        }

        [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpGet("unique/{propertyName}")]
        public async Task<ActionResult<object[]>> GetUniqueValuesAsync(
            string propertyName)
        {
            try
            {
                return this.Ok(await this.loadingUnitProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e);
            }
        }

        [ProducesResponseType(typeof(LoadingUnitDetails), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [HttpPatch("{id}")]
        public async Task<ActionResult<LoadingUnitDetails>> UpdateAsync(LoadingUnitDetails model, int id)
        {
            if (id != model?.Id)
            {
                return this.BadRequest();
            }

            var result = await this.loadingUnitProvider.UpdateAsync(model);
            if (!result.Success)
            {
                return this.NegativeResponse(result);
            }

            await this.NotifyEntityUpdatedAsync(nameof(LoadingUnit), result.Entity.Id, HubEntityOperation.Updated);

            return this.Ok(result.Entity);
        }

        [HttpPost("{id}/withdraw")]
        [ProducesResponseType(typeof(LoadingUnitSchedulerRequest), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<ActionResult<LoadingUnitSchedulerRequest>> WithdrawAsync(int id, int bayId)
        {
            var loadingUnit = await this.loadingUnitProvider.GetByIdAsync(id);
            if (loadingUnit == null)
            {
                return this.UnprocessableEntity();
            }

            var result = await this.schedulerService.WithdrawLoadingUnitAsync(id, loadingUnit.LoadingUnitTypeId, bayId);
            if (result is UnprocessableEntityOperationResult<LoadingUnitSchedulerRequest>)
            {
                return this.UnprocessableEntity(new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Detail = result.Description
                });
            }

            await this.NotifyEntityUpdatedAsync(nameof(SchedulerRequest), -1, HubEntityOperation.Created);
            await this.NotifyEntityUpdatedAsync(nameof(Mission), -1, HubEntityOperation.Created);
            await this.NotifyEntityUpdatedAsync(nameof(LoadingUnit), id, HubEntityOperation.Updated);

            return this.CreatedAtAction(nameof(this.WithdrawAsync), new { id = result.Entity.Id }, result.Entity);
        }

        #endregion
    }
}
