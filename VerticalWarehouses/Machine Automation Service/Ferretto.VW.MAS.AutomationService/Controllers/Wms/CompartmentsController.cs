using System;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/[controller]")]
    [ApiController]
    public class CompartmentsController : ControllerBase
    {
        #region Fields

        private readonly ICompartmentsWmsWebService compartmentsWmsWebService;

        #endregion

        #region Constructors

        public CompartmentsController(ICompartmentsWmsWebService compartmentsWmsWebService)
        {
            this.compartmentsWmsWebService = compartmentsWmsWebService ?? throw new ArgumentNullException(nameof(compartmentsWmsWebService));
        }

        #endregion

        #region Methods

        [HttpPut("{id}/items/{itemId}/stock")]
        public async Task<IActionResult> UpdateItemStockAsync(int id, int itemId, double stock, int? reasonId, string reasonNotes)
        {
            await this.compartmentsWmsWebService.UpdateItemStockAsync(id, itemId, stock, reasonId, reasonNotes);

            return this.Ok();
        }

        #endregion
    }
}
