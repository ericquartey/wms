using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        #region Fields

        private readonly IHubContext<OperatorHub> hubContext;

        private readonly IItemsWmsWebService itemsWmsWebService;

        #endregion

        #region Constructors

        public ItemsController(IItemsWmsWebService itemsWmsWebService, IHubContext<OperatorHub> hubContext)
        {
            this.itemsWmsWebService = itemsWmsWebService ?? throw new ArgumentNullException(nameof(itemsWmsWebService));
            this.hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        #endregion

        #region Methods

        [HttpPost("/barcodes/{code}")]
        public async Task<ActionResult<Item>> GetByBarcodeAsync(string code)
        {
            return this.Ok(await this.itemsWmsWebService.GetByBarcodeAsync(code));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDetails>> GetByIdAsync(int id)
        {
            return this.Ok(await this.itemsWmsWebService.GetByIdAsync(id));
        }

        [HttpPost("{id}/pick")]
        public async Task<IActionResult> PickAsync(int id, ItemOptions itemOptions)
        {
            _ = await this.itemsWmsWebService.PickAsync(id, itemOptions);

            await this.hubContext.Clients.All.SendAsync(nameof(IOperatorHub.ProductsChanged));

            return this.Ok();
        }

        [HttpPost("{id}/put")]
        public async Task<IActionResult> PutAsync(int id, ItemOptions itemOptions)
        {
            _ = await this.itemsWmsWebService.PutAsync(id, itemOptions);

            await this.hubContext.Clients.All.SendAsync(nameof(IOperatorHub.ProductsChanged));

            return this.Ok();
        }

        #endregion
    }
}
