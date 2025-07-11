using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/[controller]")]
    [ApiController]
    public class ItemListsController : ControllerBase
    {
        #region Fields

        private readonly IItemListsWmsWebService itemListsWmsWebService;

        #endregion

        #region Constructors

        public ItemListsController(IItemListsWmsWebService itemListsWmsWebService)
        {
            this.itemListsWmsWebService = itemListsWmsWebService ?? throw new ArgumentNullException(nameof(itemListsWmsWebService));
        }

        #endregion

        #region Methods

        [HttpGet("{id}/execute")]
        public async Task<ActionResult<ItemListDetails>> ExecuteAsync(int id, int areaId, ItemListEvadabilityType type, int? bayId = null, string userName = null)
        {
            await this.itemListsWmsWebService.ExecuteAsync(id, areaId, type, bayId, userName);
            return this.Ok();
        }

        [HttpGet("{id}/execute-num")]
        public async Task<ActionResult<ItemListDetails>> ExecuteNumAsync(string id, int areaId, ItemListEvadabilityType type, int? bayId = null, string userName = null)
        {
            await this.itemListsWmsWebService.ExecuteNumAsync(id, areaId, type, bayId, userName);
            return this.Ok();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemListDetails>> GetByIdAsync(int id)
        {
            return this.Ok(await this.itemListsWmsWebService.GetByIdAsync(id));
        }

        [HttpGet("{code}/num")]
        public async Task<ActionResult<IEnumerable<ItemList>>> GetByNumAsync(string code)
        {
            return this.Ok(await this.itemListsWmsWebService.GetAllAsync(where: code));
        }

        [HttpGet("{id}/rows")]
        public async Task<ActionResult<IEnumerable<ItemListRow>>> GetRowsAsync(int id)
        {
            return this.Ok(await this.itemListsWmsWebService.GetRowsAsync(id));
        }

        /// <summary>
        /// Mette in attesa una lista tramite l'Id in modo da porterla metteren in esecuzione in seguito
        /// </summary>
        /// <param name="id">Id della Lista che si vuole sospendere</param>
        /// <param name="userName"></param>
        /// <returns></returns>
        [HttpPost("{id}/suspend-list")]
        public async Task<ActionResult> SuspendAsync(int id, string userName = null)
        {
            await this.itemListsWmsWebService.SuspendAsync(id, userName);

            return this.Ok();
        }

        #endregion
    }
}
