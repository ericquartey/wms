using System;
using System.Collections.Generic;
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

        [HttpGet("{id}/item-lists")]
        public async Task<ActionResult<IEnumerable<ItemList>>> GetItemListsAsync(int id)
        {
            return this.Ok(await this.areasWmsWebService.GetItemListsAsync(id));
        }

        [HttpGet("{id}/items")]
        public async Task<ActionResult<IEnumerable<Item>>> GetItemsAsync(int id, int? skip = null, int? take = null, string where = null, string orderBy = null, string search = null)
        {
            return this.Ok(await this.areasWmsWebService.GetItemsAsync(id, skip, take, where, orderBy, search));
        }

        #endregion
    }
}
