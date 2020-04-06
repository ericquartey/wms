using System;
using System.Threading.Tasks;
using Ferretto.VW.MAS.AutomationService.Hubs;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/[controller]")]
    [ApiController]
    public class CompartmentsController : ControllerBase
    {
        #region Fields

        private readonly ICompartmentsWmsWebService compartmentsWmsWebService;

        private readonly IHubContext<OperatorHub> hubContext;

        #endregion

        #region Constructors

        public CompartmentsController(
            ICompartmentsWmsWebService compartmentsWmsWebService,
            IHubContext<OperatorHub> hubContext)
        {
            this.compartmentsWmsWebService = compartmentsWmsWebService ?? throw new ArgumentNullException(nameof(compartmentsWmsWebService));
            this.hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        }

        #endregion

        #region Methods

        [HttpPut("{id}/items/{itemId}/stock")]
        public async Task<IActionResult> UpdateItemStockAsync(int id, int itemId, double stock, int? reasonId, string reasonNotes)
        {
            await this.compartmentsWmsWebService.UpdateItemStockAsync(id, itemId, stock, reasonId, reasonNotes);

            await this.hubContext.Clients.All.SendAsync(nameof(IOperatorHub.ProductsChanged));

            return this.Ok();
        }

        #endregion
    }
}
