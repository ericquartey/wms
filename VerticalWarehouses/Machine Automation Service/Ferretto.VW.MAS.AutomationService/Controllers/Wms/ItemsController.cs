using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

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

        [HttpGet("{id}/compartments")]
        public async Task<ActionResult<IEnumerable<Compartment>>> GetCompartmentsAsync(int id)
        {
            return this.Ok(await this.itemsWmsWebService.GetCompartmentsAsync(id));
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

        [HttpGet("{code}/signal-defect")]
        public async Task<ActionResult<bool>> SignallingDefectOnDraperyItem(string code, double goodQuantity, double wastedQuantity)
        {
            return this.Ok(await this.itemsWmsWebService.SignallingDefectOnDraperyItemAsync(code, goodQuantity, wastedQuantity));
        }

        [HttpPut("{id}/average-weight")]
        public async Task<IActionResult> UpdateAverageWeight(int id, double weight)
        {
            await this.itemsWmsWebService.UpdateAverageWeightAsync(id, weight);

            await this.hubContext.Clients.All.SendAsync(nameof(IOperatorHub.ProductsChanged));

            return this.Ok();
        }

        #endregion
    }
}
