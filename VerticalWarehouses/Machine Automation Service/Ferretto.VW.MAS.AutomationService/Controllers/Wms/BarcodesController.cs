using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ferretto.VW.MAS.DataLayer;
using Ferretto.WMS.Data.WebAPI.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace Ferretto.VW.MAS.AutomationService.Controllers
{
    [Route("api/wms/[controller]")]
    [ApiController]
    public class BarcodesController : ControllerBase
    {
        #region Fields

        private readonly IBarcodesWmsWebService barcodesWmsWebService;

        #endregion

        #region Constructors

        public BarcodesController(IBarcodesWmsWebService barcodesWmsWebService)
        {
            this.barcodesWmsWebService = barcodesWmsWebService ?? throw new ArgumentNullException(nameof(barcodesWmsWebService));
        }

        #endregion

        #region Methods

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BarcodeRule>>> GetAllAsync([FromServices] IWmsSettingsProvider wmsSettingsProvider)
        {
            if (wmsSettingsProvider is null)
            {
                throw new ArgumentNullException(nameof(wmsSettingsProvider));
            }
            if (wmsSettingsProvider.IsEnabled)
            {
                return this.Ok(await this.barcodesWmsWebService.GetAllAsync());
            }
            else
            {
                throw new ApplicationException("WMS disabled");
            }
        }

        #endregion
    }
}
