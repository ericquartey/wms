using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.Common.BusinessModels;
using Ferretto.Common.EF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ferretto.WMS.Scheduler.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemListRowsController : ControllerBase
    {
        #region Fields

        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly Core.IWarehouse warehouse;

        #endregion Fields

        #region Constructors

        public ItemListRowsController(
            IServiceProvider serviceProvider,
            ILogger<ItemListRowsController> logger,
            Core.IWarehouse warehouse)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
            this.warehouse = warehouse;
        }

        #endregion Constructors

        #region Methods

        [HttpPost(nameof(Execute))]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<ActionResult> Execute(ListRowExecutionRequest request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                var acceptedRequest = await this.warehouse.PrepareListRowForExecutionAsync(request.ListId, request.AreaId, request.BayId);
                if (acceptedRequest == null)
                {
                    this.logger.LogWarning($"Request of execution for list row (id={request.ListId}) could not be processed.");

                    return this.UnprocessableEntity(this.ModelState);
                }

                this.logger.LogInformation($"Request of execution for list row (id={request.ListId}) was accepted.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An error occurred while processing the execution request for list row (id={request.ListId}).");
                return this.BadRequest(ex.Message);
            }

            return this.Ok();
        }

        [ProducesResponseType(200, Type = typeof(ItemListRow))]
        [ProducesResponseType(400)]
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                using (var dbContext = (DatabaseContext)this.serviceProvider.GetService(typeof(DatabaseContext)))
                {
                    var result = await dbContext.ItemListRows
                        //.Include(l => l.ItemListRows)
                        .Select(i => new ItemListRowDetails
                        {
                            Id = i.Id,
                            Code = i.Code,
                            CreationDate = i.CreationDate,
                            ItemDescription = i.Item.Description,
                            RowPriority = i.Priority
                        }
                        ).ToListAsync();

                    return this.Ok(result);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An error occurred while retrieving the list of lists.");
                return this.BadRequest(ex.Message);
            }
        }

        [ProducesResponseType(200, Type = typeof(IEnumerable<ItemListRow>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                using (var dbContext = (DatabaseContext)this.serviceProvider.GetService(typeof(DatabaseContext)))
                {
                    var listRow = await dbContext.ItemListRows
                       .Include(r => r.Item)
                       .Where(l => l.Id == id)
                       .Select(l => new ItemListRowDetails
                       {
                           Id = l.Id,
                           Code = l.Code,
                           ItemDescription = l.Item.Description,
                           RowPriority = l.Priority,
                           ItemListRowStatus = (ItemListRowStatus)l.Status,
                           DispatchedQuantity = l.DispatchedQuantity,
                           ItemId = l.ItemId,
                           CreationDate = l.CreationDate,
                           LastModificationDate = l.LastModificationDate,
                           CompletionDate = l.CompletionDate,
                           LastExecutionDate = l.LastExecutionDate,
                           Lot = l.Lot,
                           MaterialStatusId = l.MaterialStatusId,
                           PackageTypeId = l.PackageTypeId,
                           RegistrationNumber = l.RegistrationNumber,
                           RequiredQuantity = l.RequiredQuantity,
                           Sub1 = l.Sub1,
                           Sub2 = l.Sub2
                       })
                       .SingleOrDefaultAsync();

                    if (listRow == null)
                    {
                        this.logger.LogError($"No List row with (id={id}) exists.");
                        return this.NotFound();
                    }

                    //listRow.ItemListStatusChoices = ((ItemListStatus[])Enum.GetValues(typeof(ItemListStatus)))
                    //    .Select(i => new Enumeration((int)i, i.ToString()))
                    //    .ToList();

                    //listRow.ItemListTypeChoices = ((ItemListType[])Enum.GetValues(typeof(ItemListType)))
                    //    .Select(i => new Enumeration((int)i, i.ToString()))
                    //    .ToList();

                    return this.Ok(listRow);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An error occurred while retrieving the list row (id={id}).");
                return this.BadRequest(ex.Message);
            }
        }

        #endregion Methods
    }
}
