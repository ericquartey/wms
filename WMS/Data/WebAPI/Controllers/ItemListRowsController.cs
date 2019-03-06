using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.Common.Utils.Expressions;
using Ferretto.WMS.Data.Core.Extensions;
using Ferretto.WMS.Data.Core.Interfaces;
using Ferretto.WMS.Data.Core.Models;
using Ferretto.WMS.Data.WebAPI.Interfaces;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Data.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemListRowsController :
        ControllerBase,
        ICreateController<ItemListRowDetails>,
        IReadAllPagedController<ItemListRow>,
        IReadSingleController<ItemListRowDetails, int>,
        IUpdateController<ItemListRowDetails>,
        IGetUniqueValuesController
    {
        #region Fields

        private readonly IItemListRowProvider itemListRowProvider;

        private readonly Scheduler.Core.Interfaces.IItemListRowSchedulerProvider itemListRowSchedulerProvider;

        private readonly ILogger logger;

        #endregion

        #region Constructors

        public ItemListRowsController(
            ILogger<ItemListRowsController> logger,
            Scheduler.Core.Interfaces.IItemListRowSchedulerProvider itemListRowSchedulerProvider,
            IItemListRowProvider itemListRowProvider)
        {
            this.logger = logger;
            this.itemListRowSchedulerProvider = itemListRowSchedulerProvider;
            this.itemListRowProvider = itemListRowProvider;
        }

        #endregion

        #region Methods

        [ProducesResponseType(201, Type = typeof(ItemListRowDetails))]
        [ProducesResponseType(400)]
        [HttpPost]
        public async Task<ActionResult<ItemListRowDetails>> CreateAsync(ItemListRowDetails model)
        {
            var result = await this.itemListRowProvider.CreateAsync(model);

            if (!result.Success)
            {
                return this.BadRequest();
            }

            return this.Created(this.Request.GetUri(), result.Entity);
        }

        [HttpPost("execute")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<ActionResult> ExecuteAsync(Scheduler.Core.ListRowExecutionRequest request)
        {
            if (request == null)
            {
                return this.BadRequest();
            }

            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                var acceptedRequest = await this.itemListRowSchedulerProvider.PrepareForExecutionAsync(request);
                if (acceptedRequest == null)
                {
                    this.logger.LogWarning($"Request of execution for list row (id={request.ListRowId}) could not be processed.");

                    return this.UnprocessableEntity(this.ModelState);
                }

                this.logger.LogInformation($"Request of execution for list row (id={request.ListRowId}) was accepted.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An error occurred while processing the execution request for list row (id={request.ListRowId}).");
                return this.BadRequest(ex.Message);
            }

            return this.Ok();
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<ItemListRow>))]
        [ProducesResponseType(400, Type = typeof(string))]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemListRow>>> GetAllAsync(
            int skip = 0,
            int take = int.MaxValue,
            string where = null,
            string orderBy = null,
            string search = null)
        {
            try
            {
                return this.Ok(
                    await this.itemListRowProvider.GetAllAsync(
                        skip,
                        take,
                        orderBy.ParseSortOptions(),
                        where,
                        search));
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(int))]
        [ProducesResponseType(400, Type = typeof(string))]
        [HttpGet("count")]
        public async Task<ActionResult<int>> GetAllCountAsync(string where = null, string search = null)
        {
            try
            {
                return await this.itemListRowProvider.GetAllCountAsync(where, search);
            }
            catch (NotSupportedException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(ItemListRowDetails))]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemListRowDetails>> GetByIdAsync(int id)
        {
            var result = await this.itemListRowProvider.GetByIdAsync(id);
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
                return this.Ok(await this.itemListRowProvider.GetUniqueValuesAsync(propertyName));
            }
            catch (InvalidOperationException e)
            {
                return this.BadRequest(e.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(ItemListRowDetails))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpPatch]
        public async Task<ActionResult<ItemListRowDetails>> UpdateAsync(ItemListRowDetails model)
        {
            if (model == null)
            {
                return this.BadRequest();
            }

            var result = await this.itemListRowProvider.UpdateAsync(model);
            if (!result.Success)
            {
                if (result is NotFoundOperationResult<ItemListRowDetails>)
                {
                    return this.NotFound();
                }

                return this.BadRequest();
            }

            return this.Ok(result.Entity);
        }

        #endregion
    }
}
