using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/[controller]")]
    [ApiController]
    public class AreasController : ControllerBase
    {
        #region Fields

        private readonly IAreasWmsWebService areasWmsWebService;

        #endregion

        #region Constructors

        public AreasController(IAreasWmsWebService areasWmsWebService)
        {
            this.areasWmsWebService = areasWmsWebService ?? throw new ArgumentNullException(nameof(areasWmsWebService));
        }

        #endregion

        #region Methods

        [HttpGet("all-products")]
        public async Task<ActionResult<IEnumerable<ProductInMachine>>> GetAllProductsAsync(string search = null)
        {
            return this.Ok(await this.areasWmsWebService.GetAllProductsAsync(search));
        }

        [HttpGet("{id}/item-lists")]
        public async Task<ActionResult<IEnumerable<ItemList>>> GetItemListsAsync(int id, int machineId, int bayNumber)
        {
            return this.Ok(await this.areasWmsWebService.GetItemListsAsync(id, machineId, bayNumber));
        }

        [HttpGet("{id}/products")]
        public async Task<ActionResult<IEnumerable<ProductInMachine>>> GetProductsAsync(int id, int skip = 0, int? take = null, string search = null, bool groupByLot = false, bool distinctBySerialNumber = false)
        {
            return this.Ok(await this.areasWmsWebService.GetProductsAsync(id, skip, take ?? 60, search, groupByLot, distinctBySerialNumber));
        }

        #endregion
    }
}
