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
    public class ItemListsController : ControllerBase
    {
        #region Fields

        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;
        private readonly Core.IWarehouse warehouse;

        #endregion Fields

        #region Constructors

        public ItemListsController(
            IServiceProvider serviceProvider,
            ILogger<ItemListsController> logger,
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
        public async Task<ActionResult> Execute(ListExecutionRequest request)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest(this.ModelState);
            }

            try
            {
                var acceptedRequest = await this.warehouse.PrepareListForExecutionAsync(request.ListId, request.AreaId, request.BayId);
                if (acceptedRequest == null)
                {
                    this.logger.LogWarning($"Request of execution for list (id={request.ListId}) could not be processed.");

                    return this.UnprocessableEntity(this.ModelState);
                }

                this.logger.LogInformation($"Request of execution for list (id={request.ListId}) was accepted.");
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An error occurred while processing the execution request for list (id={request.ListId}).");
                return this.BadRequest(ex.Message);
            }

            return this.Ok();
        }

        [ProducesResponseType(200, Type = typeof(ItemList))]
        [ProducesResponseType(400)]
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            try
            {
                using (var dbContext = (DatabaseContext)this.serviceProvider.GetService(typeof(DatabaseContext)))
                {
                    var result = await dbContext.ItemLists
                        .Include(l => l.ItemListRows)
                        .Select(i => new ItemList
                        {
                            Id = i.Id,
                            Code = i.Code,
                            CreationDate = i.CreationDate,
                            Description = i.Description,
                            ItemListRowsCount = i.ItemListRows.Count(),
                            ItemListItemsCount = i.ItemListRows.Sum(r => r.RequiredQuantity),
                            Priority = i.Priority
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

        [ProducesResponseType(200, Type = typeof(IEnumerable<ItemList>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            try
            {
                using (var dbContext = (DatabaseContext)this.serviceProvider.GetService(typeof(DatabaseContext)))
                {
                    var list = await dbContext.ItemLists
                       .Include(l => l.ItemListRows)
                       .ThenInclude(r => r.Item)
                       .Where(l => l.Id == id)
                       .Select(l => new ItemListDetails
                       {
                           Id = l.Id,
                           Code = l.Code,
                           Description = l.Description,
                           Priority = l.Priority,
                           ItemListStatus = (ItemListStatus)l.Status,
                           ItemListType = (ItemListType)l.ItemListType,
                           ItemListItemsCount = l.ItemListRows.Sum(row => row.RequiredQuantity),
                           CreationDate = l.CreationDate,
                           Job = l.Job,
                           CustomerOrderCode = l.CustomerOrderCode,
                           CustomerOrderDescription = l.CustomerOrderDescription,
                           ShipmentUnitAssociated = l.ShipmentUnitAssociated,
                           ShipmentUnitCode = l.ShipmentUnitCode,
                           ShipmentUnitDescription = l.ShipmentUnitDescription,
                           LastModificationDate = l.LastModificationDate,
                           FirstExecutionDate = l.FirstExecutionDate,
                           ExecutionEndDate = l.ExecutionEndDate,
                           ItemListRows = l.ItemListRows.Select(r => new ItemListRow
                           {
                               Id = r.Id,
                               Code = r.Code,
                               CreationDate = r.CreationDate,
                               ItemDescription = r.Item.Description,
                               RequiredQuantity = r.RequiredQuantity,
                               RowPriority = r.Priority
                           })
                       })
                       .SingleOrDefaultAsync();

                    if (list == null)
                    {
                        this.logger.LogError($"No List with (id={id}) exists.");
                        return this.NotFound();
                    }

                    list.ItemListStatusChoices = ((ItemListStatus[])Enum.GetValues(typeof(ItemListStatus)))
                        .Select(i => new Enumeration((int)i, i.ToString()))
                        .ToList();

                    list.ItemListTypeChoices = ((ItemListType[])Enum.GetValues(typeof(ItemListType)))
                        .Select(i => new Enumeration((int)i, i.ToString()))
                        .ToList();

                    return this.Ok(list);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"An error occurred while retrieving the list (id={id}).");
                return this.BadRequest(ex.Message);
            }
        }

        #endregion Methods
    }
}
